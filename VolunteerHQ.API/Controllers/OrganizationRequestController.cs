using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunteerHQ.Core.DTOs.OrganizationRequestDTOs;
using VolunteerHQ.Infrastructure.Services;

namespace VolunteerHQ.API.Controllers;


[ApiController]
[Route("/api/[controller]")]
public class OrganizationRequestController : ControllerBase
{
    
    private readonly OrganizationRequestService _orgReqService;

    public OrganizationRequestController(OrganizationRequestService orgReqService)
    {
        _orgReqService = orgReqService;
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] CreateOrganizationRequestDto dto, CancellationToken ct = default )
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _orgReqService.CreateRequest(userId, dto, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{requestId}")]
    public async Task<IActionResult> GetRequest(int requestId , CancellationToken ct = default)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _orgReqService.GetCreateRequest(userId, requestId , ct);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllRequests(CancellationToken ct = default)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _orgReqService.GetAllRequests(userId, ct);

        return Ok(result);
    }

    [Authorize]
    [HttpPut("{requestId}/review")]
    public async Task<IActionResult> ReviewRequest(int requestId ,  [FromBody] ReviewOrganizationRequestDto dto, CancellationToken ct = default)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _orgReqService.ReviewOrganizationRequest(userId, requestId, dto , ct);

        return Ok(result);
    }
}