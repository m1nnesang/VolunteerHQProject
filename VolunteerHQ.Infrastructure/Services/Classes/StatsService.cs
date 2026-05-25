using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.StatsDTOs;
using VolunteerHQ.Core.Enums;
using VolunteerHQ.Infrastructure.Data;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.Infrastructure.Services.Classes;

public class StatsService : IStatsService
{
    private readonly AppDbContext _db;
    private readonly ValidatorService _vs;

    public StatsService(AppDbContext db, ValidatorService vs)
    {
        _db = db;
        _vs = vs;
    }

    public async Task<StatsResponseDto> GetStats(CancellationToken ct = default)
    {
        var activeFundraisers = await _db.Fundraisers
            .CountAsync(f => f.Status == FundraiserStatus.Open || f.Status == FundraiserStatus.InProgress, ct);

        var completedFundraisers = await _db.Fundraisers
            .CountAsync(f => f.Status == FundraiserStatus.Completed, ct);

        var totalDonated = await _db.Donations
            .SumAsync(d => (decimal?)d.Amount, ct) ?? 0m;

        var uniqueDonors = await _db.Donations
            .Where(d => d.UserId != null)
            .Select(d => d.UserId)
            .Distinct()
            .CountAsync(ct);

        var organizationsCount = await _db.Organizations.CountAsync(ct);

        var militaryUnitsCount = await _db.MilitaryUnits.CountAsync(ct);

        return new StatsResponseDto(
            activeFundraisers,
            completedFundraisers,
            totalDonated,
            uniqueDonors,
            organizationsCount,
            militaryUnitsCount);
    }

    public async Task<FundraiserStatsResponseDto> GetFundraiserStats(int fundraiserId, CancellationToken ct = default)
    {
        await _vs.GetFundraiserOrThrow(fundraiserId, ct);

        var currentProgress = await _db.Donations
            .Where(d => d.FundraiserId == fundraiserId)
            .SumAsync(d => (decimal?)d.Amount, ct) ?? 0m;

        var donorsCount = await _db.Donations
            .Where(d => d.FundraiserId == fundraiserId && d.UserId != null)
            .Select(d => d.UserId)
            .Distinct()
            .CountAsync(ct);

        var topDonorsRaw = await _db.Donations
            .Where(d => d.FundraiserId == fundraiserId)
            .GroupBy(d => d.UserId)
            .Select(g => new { UserId = g.Key, Total = g.Sum(d => d.Amount) })
            .OrderByDescending(x => x.Total)
            .Take(5)
            .ToListAsync(ct);

        var topDonors = topDonorsRaw
            .Select(x => new TopDonorDto(x.UserId, x.Total))
            .ToList();

        return new FundraiserStatsResponseDto(fundraiserId, currentProgress, donorsCount, topDonors);
    }
}
