using Microsoft.AspNetCore.Mvc;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.API.Controllers;

[ApiController]
[Route("api/stats")]
public class StatsController : ControllerBase
{
    private readonly IStatsService _statsService;

    public StatsController(IStatsService statsService)
    {
        _statsService = statsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStats(CancellationToken ct = default)
    {
        var result = await _statsService.GetStats(ct);
        return Ok(result);
    }

    [HttpGet("fundraiser/{fundraiserId}")]
    public async Task<IActionResult> GetFundraiserStats(int fundraiserId, CancellationToken ct = default)
    {
        var result = await _statsService.GetFundraiserStats(fundraiserId, ct);
        return Ok(result);
    }
}
