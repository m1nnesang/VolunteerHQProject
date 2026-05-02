using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunteerHQ.Core.DTOs.CommentsDTOs;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.API.Controllers;

[ApiController]
[Route("api/fundraiser/{fundraiserId}/comments")]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpPost] 
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateCommentDto dto, int fundraiserId,
        CancellationToken ct = default)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        var result = await _commentService.CreateComment(userId, fundraiserId, dto, ct);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetComents(int fundraiserId, [FromQuery] PaginationDto pagination,
        CancellationToken ct = default)
    {
        var result = await _commentService.GetCommentsByFundraiser(fundraiserId, pagination, ct);
        return Ok(result);
    }

    [HttpDelete("{commentId}")]
    [Authorize]
    public async Task<IActionResult> Delete(int fundraiserId,int commentId ,  CancellationToken ct = default)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        await _commentService.DeleteComment(userId, commentId, ct);

        return NoContent();
    }
}