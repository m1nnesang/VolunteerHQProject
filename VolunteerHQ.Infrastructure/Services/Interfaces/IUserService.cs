using VolunteerHQ.Core.DTOs.UserDTOs;

namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> GetMe(int userId, CancellationToken ct = default);
    Task<UserStatsDto> GetMyStats(int userId, CancellationToken ct = default);
}
