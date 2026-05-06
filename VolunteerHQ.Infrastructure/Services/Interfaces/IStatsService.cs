using VolunteerHQ.Core.DTOs.StatsDTOs;

namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface IStatsService
{
    Task<StatsResponseDto> GetStats(CancellationToken ct = default);
    Task<FundraiserStatsResponseDto> GetFundraiserStats(int fundraiserId, CancellationToken ct = default);
}
