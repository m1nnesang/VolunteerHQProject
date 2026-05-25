using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : BaseController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var result = await _userService.GetMe(CurrentUserId);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me/stats")]
    public async Task<IActionResult> GetMyStats(CancellationToken ct)
    {
        var result = await _userService.GetMyStats(CurrentUserId, ct);
        return Ok(result);
    }
}