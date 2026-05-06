using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.MessageDTOs;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.API.Controllers;


[ApiController]
[Route("api/message")]
public class PrivateMessageController : BaseController
{
    private readonly IPrivateMessageService _privateMessageService;
    
    public PrivateMessageController(IPrivateMessageService privateMessageService)
    {
        _privateMessageService = privateMessageService;
    }
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Send ([FromBody] CreatePrivateMessageDto dto , CancellationToken ct = default)
    {
        var userId = CurrentUserId;
        
        var result = await _privateMessageService.SendMessage(userId, dto, ct);

        return Ok(result);
    }

    [HttpGet("{otherUserId}")]
    [Authorize]
    public async Task<IActionResult> Get(int otherUserId , [FromQuery] PaginationDto pagination, CancellationToken ct = default)
    {
        var userId = CurrentUserId;
        
        var result = await _privateMessageService.GetMessages(userId, otherUserId, pagination, ct);
        return Ok(result);
    }

    [HttpDelete("{messageId}")]
    [Authorize]
    public async Task<IActionResult> Delete(int messageId, CancellationToken ct = default)
    {
        var senderId = CurrentUserId;

        await _privateMessageService.DeleteMessage(senderId, messageId, ct);
        
        return NoContent();
    }

    [HttpPut("{messageId}/read")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(int messageId, CancellationToken ct = default)
    {
        var userId = CurrentUserId;
        
        await _privateMessageService.MarkAsRead(userId, messageId, ct);
        
        return NoContent();
    }

    [HttpPut("{messageId}")]
    [Authorize]
    public async Task<IActionResult> Update(int messageId, UpdatePrivateMessageDto dto,
        CancellationToken ct = default)
    {
        var senderId = CurrentUserId;
        
        var result = await _privateMessageService.UpdateMessage(senderId, messageId, dto, ct);
        
        return Ok(result);
    }
}