using VolunteerHQ.Core.DTOs.UserDTOs;

namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> GetMe(int userId, CancellationToken ct = default);
}
