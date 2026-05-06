using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunteerHQ.Core.DTOs.CommentsDTOs;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.API.Controllers;

[ApiController]
[Route("api/fundraiser/{fundraiserId}/comments")]
public class CommentController : BaseController
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
        var userId = CurrentUserId;

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
        var userId = CurrentUserId;

        await _commentService.DeleteComment(userId, commentId, ct);

        return NoContent();
    }
}