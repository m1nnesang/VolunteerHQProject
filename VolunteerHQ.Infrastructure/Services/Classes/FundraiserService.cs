using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.DonationDTOs;
using VolunteerHQ.Core.DTOs.FundraiserDTOs;
using VolunteerHQ.Core.Enums;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Core.Models;
using VolunteerHQ.Infrastructure.Data;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.Infrastructure.Services.Classes;

public class FundraiserService : IFundraiserService
{
    private readonly AppDbContext _db;
    private readonly ValidatorService _vs;
    private readonly INotificationService _ns;

    public FundraiserService(AppDbContext db, ValidatorService vs, INotificationService ns)
    {
        _db = db;
        _vs = vs;
        _ns = ns;
    }

    public async Task<FundraiserResponseDto> CreateFundraiser(int unitId, CreateFundraiserDto dto, CancellationToken ct = default)
    {
        await _vs.GetUnitOrThrow(unitId, ct);

        var fundraiser = new FundraiserModel
        {
            MilitaryUnitId = unitId,
            Status = FundraiserStatus.Open,
            Title = dto.Title,
            Importance = dto.Importance,
            Deadline = dto.Deadline,
            Description = dto.Description,
            Assignments = new List<FundraiserAssignmentModel>(),
            TotalGoal = dto.TotalGoal,
            CreatedAt = DateTime.UtcNow
        };

        await _db.Fundraisers.AddAsync(fundraiser, ct);
        await _db.SaveChangesAsync(ct);

        return new FundraiserResponseDto(fundraiser.Id, fundraiser.MilitaryUnitId, fundraiser.Title,
            fundraiser.Description, fundraiser.TotalGoal, 0m, fundraiser.Importance,
            fundraiser.Status, new List<FundraiserAssignmentResponseDto>(),
            fundraiser.Deadline, fundraiser.CreatedAt);
    }

    public async Task<FundraiserResponseDto> GetFundraiser(int fundraiserId, CancellationToken ct = default)
    {
        var fundraiser = await _vs.GetFundraiserOrThrow(fundraiserId, ct);

        var currentProgress = await _db.Donations
            .Where(d => d.FundraiserId == fundraiserId)
            .SumAsync(d => (decimal?)d.Amount, ct) ?? 0m;

        var assignmentAmounts = await _db.Donations
            .Where(d => d.FundraiserId == fundraiserId && d.FundraiserAssignmentId != null)
            .GroupBy(d => d.FundraiserAssignmentId!.Value)
            .Select(g => new { AssignmentId = g.Key, Total = g.Sum(d => d.Amount) })
            .ToDictionaryAsync(x => x.AssignmentId, x => x.Total, ct);

        var assignments = fundraiser.Assignments
            .Select(a => new FundraiserAssignmentResponseDto(
                a.Id, assignmentAmounts.GetValueOrDefault(a.Id, 0m),
                a.OrganizationId, a.UniqueCode, a.TakenAt))
            .ToList();

        return new FundraiserResponseDto(fundraiser.Id, fundraiser.MilitaryUnitId, fundraiser.Title,
            fundraiser.Description, fundraiser.TotalGoal, currentProgress, fundraiser.Importance,
            fundraiser.Status, assignments, fundraiser.Deadline, fundraiser.CreatedAt);
    }

    public async Task<PagedResponseDto<FundraiserResponseDto>> GetAllFundraisers(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var total = await _db.Fundraisers.CountAsync(ct);

        var fundraisers = await _db.Fundraisers
            .Include(f => f.Assignments)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        var fundraiserIds = fundraisers.Select(f => f.Id).ToList();

        var progressMap = await _db.Donations
            .Where(d => fundraiserIds.Contains(d.FundraiserId))
            .GroupBy(d => d.FundraiserId)
            .Select(g => new { FundraiserId = g.Key, Total = g.Sum(d => d.Amount) })
            .ToDictionaryAsync(x => x.FundraiserId, x => x.Total, ct);

        var assignmentAmountMap = await _db.Donations
            .Where(d => fundraiserIds.Contains(d.FundraiserId) && d.FundraiserAssignmentId != null)
            .GroupBy(d => d.FundraiserAssignmentId!.Value)
            .Select(g => new { AssignmentId = g.Key, Total = g.Sum(d => d.Amount) })
            .ToDictionaryAsync(x => x.AssignmentId, x => x.Total, ct);

        var items = fundraisers.Select(f => new FundraiserResponseDto(
            f.Id, f.MilitaryUnitId, f.Title, f.Description, f.TotalGoal,
            progressMap.GetValueOrDefault(f.Id, 0m),
            f.Importance, f.Status,
            f.Assignments.Select(a => new FundraiserAssignmentResponseDto(
                a.Id,
                assignmentAmountMap.GetValueOrDefault(a.Id, 0m),
                a.OrganizationId, a.UniqueCode, a.TakenAt)).ToList(),
            f.Deadline, f.CreatedAt)).ToList();

        return new PagedResponseDto<FundraiserResponseDto>(items, total, page, pageSize);
    }

    public async Task<FundraiserAssignmentResponseDto> AssignOrganization(int fundraiserId, int userId, int orgId, CancellationToken ct = default)
    {
       var fundraiser = await _vs.GetFundraiserOrThrow(fundraiserId, ct);
       
       if (fundraiser.Status == FundraiserStatus.Completed)
           throw new ConflictException("Неможливо підключитись до збору");
       
       var user = await _vs.GetUserInOrganizationOrThrow(userId, orgId, ct);

        if (user.MemberRole != OrganizationMemberRole.Deputy && user.MemberRole != OrganizationMemberRole.Leader)
        {
            throw new NotEnoughRightsException("U dont have enough rights for this operation");
        }
        
        var existing = await _db.FundraiserAssignments
            .FirstOrDefaultAsync(a => a.FundraiserId == fundraiserId && a.OrganizationId == orgId, ct);

        if (existing != null) throw new ConflictException("Організація вже підключена до цього збору");

        var assignment = new FundraiserAssignmentModel
        {
            FundraiserId = fundraiserId,
            OrganizationId = orgId,
            UniqueCode = Guid.NewGuid().ToString(),
            TakenAt = DateTime.UtcNow,
        };

        await _db.FundraiserAssignments.AddAsync(assignment, ct);
        await _db.SaveChangesAsync(ct);

        return new FundraiserAssignmentResponseDto(assignment.Id, 0m, assignment.OrganizationId,
            assignment.UniqueCode, assignment.TakenAt);
    }

    public async Task<DonationResponseDto> Donate(int? userId, int fundraiserId , string uniqueCode, CreateDonationDto dto, CancellationToken ct = default)
    {
        var assignment = await _db.FundraiserAssignments
            .FirstOrDefaultAsync(a => a.UniqueCode == uniqueCode, ct);
        
        if (assignment == null) throw new NotFoundException("This assignment not found");
        
        var fundraiser = await _vs.GetFundraiserOrThrow(assignment.FundraiserId, ct);
        
        var donation = new DonationModel
        {
            FundraiserId = assignment.FundraiserId,
            FundraiserAssignmentId = assignment.Id,
            UserId = userId,
            Amount = dto.Amount,
            IsAnonymous = userId == null,
            CreatedAt = DateTime.UtcNow
        };
        
        await _db.Donations.AddAsync(donation, ct);
        await _db.SaveChangesAsync(ct);
        
        if (fundraiser.Status is not FundraiserStatus.Completed and not FundraiserStatus.Closed)
            await UpdateFundraiserStatus(fundraiser.Id, fundraiser.TotalGoal, ct);
        
        if (userId != null)
            await _ns.SendNotification(userId.Value, "Дякуємо за ваш донат!", $"/fundraiser/{fundraiserId}", ct);

        return new DonationResponseDto(donation.Id, donation.UserId, donation.Amount, donation.CreatedAt);
    }

    public async Task<DonationResponseDto> DirectDonate(int? userId, int fundraiserId, CreateDonationDto dto, CancellationToken ct = default)
    {
        var fundraiser = await _vs.GetFundraiserOrThrow(fundraiserId, ct);

        var donation = new DonationModel
        {
            FundraiserId = fundraiserId,
            FundraiserAssignmentId = null,
            UserId = userId,
            Amount = dto.Amount,
            IsAnonymous = userId == null,
            CreatedAt = DateTime.UtcNow
        };

        await _db.Donations.AddAsync(donation, ct);
        await _db.SaveChangesAsync(ct);
        
        if (fundraiser.Status is not FundraiserStatus.Completed and not FundraiserStatus.Closed)
            await UpdateFundraiserStatus(fundraiser.Id, fundraiser.TotalGoal, ct);
        
        if (userId != null)
            await _ns.SendNotification(userId.Value, "Дякуємо за ваш донат!", $"/fundraiser/{fundraiserId}", ct);

        return new DonationResponseDto(donation.Id, donation.UserId, donation.Amount, donation.CreatedAt);
        
    }

    private async Task UpdateFundraiserStatus(int fundraiserId, decimal totalGoal, CancellationToken ct)
    {
        var progress = await _db.Donations
            .Where(d => d.FundraiserId == fundraiserId)
            .SumAsync(d => d.Amount, ct);

        var currentStatus = await _db.Fundraisers
            .Where(f => f.Id == fundraiserId)
            .Select(f => f.Status).FirstAsync(ct);
        
        if (currentStatus == FundraiserStatus.Completed) return; // save from RC in status update
        
        var newStatus = progress >= totalGoal
            ? FundraiserStatus.Completed
            : FundraiserStatus.InProgress;
        

        await _db.Fundraisers
            .Where(f => f.Id == fundraiserId)
            .ExecuteUpdateAsync(s => s.SetProperty(f => f.Status, newStatus), ct);

        if (newStatus == FundraiserStatus.Completed)
        {
            var donaters = await _db.Donations
                .Where(d => d.FundraiserId == fundraiserId && d.UserId != null)
                .Select(d => d.UserId!.Value)
                .Distinct()
                .ToListAsync(ct);
            
            foreach (var donorId in donaters)
                await _ns.SendNotification(donorId, "Збір завершено!", $"/fundraiser/{fundraiserId}", ct);
        
        }
    }

}