using VolunteerHQ.Core.DTOs.AuthDTOs;

namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> Register(RegisterDto dto , CancellationToken ct = default);
    Task<AuthResponseDto> Login(LoginDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> Refresh(string refreshTokenw , CancellationToken ct = default);
}
