using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.MembershipDTOs;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class OrganizationController : BaseController
{
    private readonly IOrganizationService _orgService;

    public OrganizationController(IOrganizationService orgService)
    {
        _orgService = orgService;
    }
    
    [HttpGet("{orgId}")]
    public async Task<IActionResult> GetOrganization(int orgId, CancellationToken ct = default)
    {
        var org = await _orgService.GetOrganization(orgId, ct);
        return Ok(org);
    }

    [HttpGet("{orgId}/members")]
    public async Task<IActionResult> GetMembers(int orgId, [FromQuery] PaginationDto pagination, CancellationToken ct = default)
    {
        var members = await _orgService.GetOrganizationMembers(orgId, pagination.Page, pagination.PageSize, ct);
        return Ok(members);
    }

    [Authorize]
    [HttpDelete("{orgId}/members/{targetId}")]
    public async Task<IActionResult> RemoveMember(int orgId, int targetId, CancellationToken ct = default)
    {
        var requesterId = CurrentUserId;
        await _orgService.RemoveMember(orgId, requesterId, targetId, ct);

        return NoContent();
    }

    [Authorize]
    [HttpPut("{orgId}/members/{targetId}/role")]
    public async Task<IActionResult> UpdateMember(int orgId, int targetId, [FromBody] UpdateMemberRoleDto dto,CancellationToken ct = default)
    {
        var requesterId = CurrentUserId;
        await _orgService.UpdateMemberRole(orgId, requesterId, targetId, dto,  ct);

        return NoContent();
    }
}

