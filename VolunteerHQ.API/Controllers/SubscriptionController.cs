using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.SubscriptionDTOs;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class SubscriptionController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Subscribe([FromBody] CreateSubscriptionDto dto, CancellationToken ct = default)
    {
        var userId = CurrentUserId;

        var result = await _subscriptionService.Subscribe(userId, dto, ct);

        return Ok(result);
    }

    [HttpDelete("{subscriptionId}")]
    [Authorize]
    public async Task<IActionResult> Unsubscribe(int subscriptionId, CancellationToken ct = default)
    {
        var userId = CurrentUserId;

        await _subscriptionService.Unsubscribe(userId, subscriptionId, ct);

        return NoContent();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetSubscriptions([FromQuery] PaginationDto dto, CancellationToken ct = default)
    {
        var userId = CurrentUserId;

        var result = await _subscriptionService.GetSubscriptions(userId, dto, ct);
        
        return Ok(result);
    }
}