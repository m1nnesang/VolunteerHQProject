using Microsoft.AspNetCore.Mvc;
using VolunteerHQ.Core.DTOs.AuthDTOs;
using VolunteerHQ.Infrastructure.Services;

namespace VolunteerHQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register (RegisterDto dto)
    {
        var register = await _authService.Register(dto);

        return Ok(register);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var login = await _authService.Login(dto);

        return Ok(login);
    }
}