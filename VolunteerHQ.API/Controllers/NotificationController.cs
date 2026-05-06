using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : BaseController
{
    private readonly INotificationService _notificationService;
    
    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetNotifications([FromQuery] PaginationDto pagination, CancellationToken ct = default)
    {
        var userId = CurrentUserId;

        var result = await _notificationService.GetNotifications(userId, pagination, ct);
        
        return Ok(result);
    }

    [HttpPut("{notificationId}/read")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(int notificationId, CancellationToken ct = default)
    {
        var userId = CurrentUserId;
        
        await _notificationService.MarkAsRead(userId, notificationId, ct);
        
        return NoContent();
    }
}