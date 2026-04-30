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

    public FundraiserService(AppDbContext db, ValidatorService vs)
    {
        _db = db;
        _vs = vs;
    }

    public async Task<FundraiserResponseDto> CreateFundraiser(int unitId, CreateFundraiserDto dto, CancellationToken ct = default)
    {
        await _vs.GetUnitOrThrow(unitId, ct);

        var fundraiser = new FundraiserModel()
        {
            MilitaryUnitId = unitId,
            CurrentProgress = 0,
            Status = FundraiserStatus.Open,
            Title = dto.Title,
            Importance = dto.Importance,
            Deadline = dto.Deadline,
            Description = dto.Description,
            Assignments = new List<FundraiserAssignmentModel>(),
            TotalGoal = dto.TotalGoal
        };



        await _db.Fundraisers.AddAsync(fundraiser, ct);
        await _db.SaveChangesAsync(ct);

        return new FundraiserResponseDto(fundraiser.Id, fundraiser.MilitaryUnitId, fundraiser.Title,
            fundraiser.Description,
            fundraiser.TotalGoal, fundraiser.CurrentProgress, fundraiser.Importance,
            fundraiser.Status, new List<FundraiserAssignmentResponseDto>(),
            fundraiser.Deadline, fundraiser.CreatedAt);
    }

    public async Task<FundraiserResponseDto> GetFundraiser(int fundraiserId, CancellationToken ct = default)
    {
        var fundraiser = await _vs.GetFundraiserOrThrow(fundraiserId, ct);

        return new FundraiserResponseDto(fundraiser.Id, fundraiser.MilitaryUnitId, fundraiser.Title,
            fundraiser.Description,
            fundraiser.TotalGoal, fundraiser.CurrentProgress, fundraiser.Importance,
            fundraiser.Status, fundraiser.Assignments.Select(a => new FundraiserAssignmentResponseDto(a.Id, a.AmountRaised, a.OrganizationId , a.UniqueCode , a.TakenAt)).ToList(),
            fundraiser.Deadline, fundraiser.CreatedAt);
    }

    public async Task<PagedResponseDto<FundraiserResponseDto>> GetAllFundraisers(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var total = await _db.Fundraisers.CountAsync(ct);
        
        var items = await _db.Fundraisers
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FundraiserResponseDto(f.Id , f.MilitaryUnitId, f.Title , f.Description, f.TotalGoal, f.CurrentProgress, f.Importance, f.Status, f.Assignments.Select(a => new FundraiserAssignmentResponseDto(a.Id, a.AmountRaised, a.OrganizationId , a.UniqueCode , a.TakenAt)).ToList(), f.Deadline, f.CreatedAt ))
            .ToListAsync(ct);
        
        return new PagedResponseDto<FundraiserResponseDto>(items, total, page, pageSize);
    }

    public async Task<FundraiserAssignmentResponseDto> AssignOrganization(int fundraiserId, int userId, int orgId, CancellationToken ct = default)
    {
        var uniqueCode = Guid.NewGuid().ToString();
        
        await _vs.GetFundraiserOrThrow(fundraiserId, ct);
        var user = await _vs.GetUserInOrganizationOrThrow(userId, orgId, ct);

        if (user.MemberRole != OrganizationMemberRole.Deputy && user.MemberRole != OrganizationMemberRole.Leader)
        {
            throw new NotEnoughRightsException("U dont have enough rights for this operation");
        }
        
        var existing = await _db.FundraiserAssignments
            .FirstOrDefaultAsync(a => a.FundraiserId == fundraiserId && a.OrganizationId == orgId, ct);

        if (existing != null) throw new ConflictException("Організація вже підключена до цього збору");

        var assing = new FundraiserAssignmentModel()
        {
            FundraiserId = fundraiserId,
            OrganizationId = orgId,
            UniqueCode = uniqueCode,
            AmountRaised = 0,
            TakenAt = DateTime.UtcNow,
        };
        
        await _db.FundraiserAssignments.AddAsync(assing, ct);
        await _db.SaveChangesAsync(ct);

        return new FundraiserAssignmentResponseDto(assing.Id, assing.AmountRaised, assing.OrganizationId,
            assing.UniqueCode, assing.TakenAt);
    }

    public async Task<DonationResponseDto> Donate(int? userId, string uniqueCode, CreateDonationDto dto, CancellationToken ct = default)
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

        assignment.AmountRaised += donation.Amount;
        fundraiser.CurrentProgress += donation.Amount;
        
        if (fundraiser.Status == FundraiserStatus.Open)
            fundraiser.Status = FundraiserStatus.InProgress;
        
        if (fundraiser.CurrentProgress >= fundraiser.TotalGoal)
            fundraiser.Status = FundraiserStatus.Completed;

        await _db.AddAsync(donation, ct);
        await _db.SaveChangesAsync(ct);
        
        return new DonationResponseDto(donation.Id, donation.UserId, donation.Amount, donation.CreatedAt);
    }

    public async Task<DonationResponseDto> DirectDonate(int? userId, int fundraiserId, CreateDonationDto dto,
        CancellationToken ct = default)
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
        
        fundraiser.CurrentProgress += donation.Amount;
        
        if (fundraiser.Status == FundraiserStatus.Open)
            fundraiser.Status = FundraiserStatus.InProgress;
        
        if (fundraiser.CurrentProgress >= fundraiser.TotalGoal)
            fundraiser.Status = FundraiserStatus.Completed;

        await _db.AddAsync(donation, ct);
        await _db.SaveChangesAsync(ct);
        
        return new DonationResponseDto(donation.Id, donation.UserId, donation.Amount, donation.CreatedAt);
    }

}