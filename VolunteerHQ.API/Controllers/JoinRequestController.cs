using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.JoinRequestDTOs;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JoinRequestController : BaseController
{
    private readonly IJoinRequestService _service;

    public JoinRequestController(IJoinRequestService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost("{orgId}")]
    public async Task<IActionResult> Create(int orgId, [FromBody] CreateJoinRequestDto dto, CancellationToken ct)
    {
        var userId = CurrentUserId;
        var result = await _service.CreateJoinRequest(userId, orgId, dto, ct);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{requestId}")]
    public async Task<IActionResult> Get(int requestId, int orgId, CancellationToken ct = default)
    {
        var userId = CurrentUserId;
        var result = await _service.GetJoinRequest(requestId, userId, orgId, ct);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("org/{orgId}")]
    public async Task<IActionResult> GetAll(int orgId, [FromQuery] PaginationDto pagination, CancellationToken ct = default)
    {
        var result = await _service.GetAllJoinRequests(orgId, pagination.Page, pagination.PageSize, ct);

        return Ok(result);
    }

    [Authorize]
    [HttpPut("{requestId}/review/{orgId}")]
    public async Task<IActionResult> Review(int requestId, int orgId, [FromBody] ReviewJoinRequestDto dto, CancellationToken ct = default)
    {
        var reviewerId = CurrentUserId;
        var result = await _service.ReviewJoinRequest(dto, reviewerId, orgId, requestId , ct);

        return Ok(result);
    }
}
