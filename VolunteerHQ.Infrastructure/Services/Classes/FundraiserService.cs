using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<FundraiserService> _logger;

    public FundraiserService(AppDbContext db, ValidatorService vs, INotificationService ns , ILogger<FundraiserService> logger)
    {
        _db = db;
        _vs = vs;
        _ns = ns;
        _logger = logger;
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

        await NotifyUnitSubscribers(unitId, fundraiser.Id, fundraiser.Title, ct);

        return new FundraiserResponseDto(fundraiser.Id, fundraiser.MilitaryUnitId, fundraiser.Title,
            fundraiser.Description, fundraiser.TotalGoal, 0m, fundraiser.Importance,
            fundraiser.Status, new List<FundraiserAssignmentResponseDto>(),
            fundraiser.Deadline, fundraiser.CreatedAt);
    }

    private async Task NotifyUnitSubscribers(int unitId, int fundraiserId, string title, CancellationToken ct)
    {
        var subscriberIds = await _db.Subscriptions
            .Where(s => s.Target == SubscriptionTargetType.MilitaryUnit && s.TargetId == unitId)
            .Select(s => s.UserId)
            .ToListAsync(ct);

        foreach (var subscriberId in subscriberIds)
        {
            try
            {
                await _ns.SendNotification(subscriberId,
                    $"Новий збір: {title}", $"/fundraiser/{fundraiserId}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify subscriber {UserId} about fundraiser {FundraiserId}",
                    subscriberId, fundraiserId);
            }
        }
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
                a.OrganizationId, a.Organization?.OrganizationName,
                a.UniqueCode, a.TakenAt))
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
                .ThenInclude(a => a.Organization)
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
                a.OrganizationId, a.Organization?.OrganizationName,
                a.UniqueCode, a.TakenAt)).ToList(),
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

        var orgName = await _db.Organizations
            .Where(o => o.Id == orgId)
            .Select(o => o.OrganizationName)
            .FirstOrDefaultAsync(ct);

        return new FundraiserAssignmentResponseDto(assignment.Id, 0m, assignment.OrganizationId,
            orgName, assignment.UniqueCode, assignment.TakenAt);
    }

    public async Task<DonationResponseDto> Donate(int? userId, int fundraiserId , string uniqueCode, CreateDonationDto dto, CancellationToken ct = default)
    {
        var assignment = await _db.FundraiserAssignments
            .FirstOrDefaultAsync(a => a.UniqueCode == uniqueCode, ct);

        if (assignment == null) throw new NotFoundException("This assignment not found");

        var fundraiser = await _vs.GetFundraiserOrThrow(assignment.FundraiserId, ct);

        var donation = await PersistDonationAsync(fundraiser, assignment.Id, userId, dto.Amount, ct);

        if (fundraiser.Status is not FundraiserStatus.Completed and not FundraiserStatus.Closed)
            await UpdateFundraiserStatus(fundraiser.Id, fundraiser.TotalGoal, ct);

        try
        {
            if (userId != null)
                await _ns.SendNotification(userId.Value, "Дякуємо за ваш донат!", $"/fundraiser/{fundraiserId}", ct);
        }
        catch
        {
            _logger.LogError("Notification service error");
        }

        return new DonationResponseDto(donation.Id, donation.UserId, donation.Amount, donation.CreatedAt);
    }

    public async Task<DonationResponseDto> DirectDonate(int? userId, int fundraiserId, CreateDonationDto dto, CancellationToken ct = default)
    {
        var fundraiser = await _vs.GetFundraiserOrThrow(fundraiserId, ct);

        var donation = await PersistDonationAsync(fundraiser, null, userId, dto.Amount, ct);

        if (fundraiser.Status is not FundraiserStatus.Completed and not FundraiserStatus.Closed)
            await UpdateFundraiserStatus(fundraiser.Id, fundraiser.TotalGoal, ct);

        try
        {
            if (userId != null)
                await _ns.SendNotification(userId.Value, "Дякуємо за ваш донат!", $"/fundraiser/{fundraiserId}", ct);
        }
        catch
        {
            _logger.LogError("Notification service error");
        }

        return new DonationResponseDto(donation.Id, donation.UserId, donation.Amount, donation.CreatedAt);
    }
    
    private async Task<DonationModel> PersistDonationAsync(
        FundraiserModel fundraiser, int? assignmentId, int? userId, decimal requestedAmount, CancellationToken ct)
    {
        var isRelational = _db.Database.IsRelational();

        var transaction = isRelational
            ? await _db.Database.BeginTransactionAsync(ct)
            : null;

        try
        {
            if (isRelational)
                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT 1 FROM \"Fundraisers\" WHERE \"Id\" = {fundraiser.Id} FOR UPDATE", ct);

            var currentProgress = await _db.Donations
                .Where(d => d.FundraiserId == fundraiser.Id)
                .SumAsync(d => (decimal?)d.Amount, ct) ?? 0m;

            var remaining = fundraiser.TotalGoal - currentProgress;

            if (remaining <= 0)
                throw new ConflictException("Збір вже виконано");

            var actualAmount = Math.Min(requestedAmount, remaining);

            var donation = new DonationModel
            {
                FundraiserId = fundraiser.Id,
                FundraiserAssignmentId = assignmentId,
                UserId = userId,
                Amount = actualAmount,
                IsAnonymous = userId == null,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Donations.AddAsync(donation, ct);
            await _db.SaveChangesAsync(ct);

            if (transaction != null)
                await transaction.CommitAsync(ct);

            return donation;
        }
        finally
        {
            if (transaction != null)
                await transaction.DisposeAsync();
        }
    }

    private async Task UpdateFundraiserStatus(int fundraiserId, decimal totalGoal, CancellationToken ct)
    {
        var progress = await _db.Donations
            .Where(d => d.FundraiserId == fundraiserId)
            .SumAsync(d => d.Amount, ct);

        var fundraiser = await _db.Fundraisers
            .FirstAsync(f => f.Id == fundraiserId, ct);

        if (fundraiser.Status == FundraiserStatus.Completed) return; // save from RC in status update

        var newStatus = progress >= totalGoal
            ? FundraiserStatus.Completed
            : FundraiserStatus.InProgress;

        fundraiser.Status = newStatus;
        await _db.SaveChangesAsync(ct);

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