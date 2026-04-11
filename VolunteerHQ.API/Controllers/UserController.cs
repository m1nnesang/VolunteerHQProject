using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunteerHQ.Core.DTOs.UserDTOs;
using VolunteerHQ.Infrastructure.Services;

namespace VolunteerHQ.API.Controllers;

[ApiController]
[Route("/api[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        { 
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _userService.GetMe(userId);

            return Ok(result);
        }
    
}