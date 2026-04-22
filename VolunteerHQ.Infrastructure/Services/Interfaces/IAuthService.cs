using VolunteerHQ.Core.DTOs.AuthDTOs;

namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> Register(RegisterDto dto);
    Task<AuthResponseDto> Login(LoginDto dto);
}
