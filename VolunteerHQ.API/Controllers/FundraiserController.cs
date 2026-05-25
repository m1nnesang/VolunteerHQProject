using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.DonationDTOs;
using VolunteerHQ.Core.DTOs.FundraiserDTOs;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FundraiserController : BaseController
{
    private readonly IFundraiserService _service;

    public FundraiserController(IFundraiserService service)
    {
        _service = service;
    }

    [Authorize(Roles = "MilitaryUnit")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFundraiserDto dto, CancellationToken ct)
    {
        var unitId = CurrentUserId;
        var result = await _service.CreateFundraiser(unitId, dto, ct);
        return Ok(result);
    }

    [HttpGet("{fundraiserId}")]
    public async Task<IActionResult> Get(int fundraiserId, CancellationToken ct = default)
    {
        var result = await _service.GetFundraiser(fundraiserId, ct);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationDto pagination, CancellationToken ct = default)
    {
        var result = await _service.GetAllFundraisers(pagination.Page, pagination.PageSize, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{fundraiserId}/assign")]
    public async Task<IActionResult> Assign(int fundraiserId, int orgId, CancellationToken ct = default)
    {
        var userId = CurrentUserId;
        var result = await _service.AssignOrganization(fundraiserId, userId, orgId, ct);
        return Ok(result);
    }

    [HttpPost("donate/{uniqueCode}")]
    public async Task<IActionResult> Donate(string uniqueCode, int fundraiserId,  [FromBody] CreateDonationDto dto, CancellationToken ct = default)
    {
        var userId = User.Identity?.IsAuthenticated == true
            ? CurrentUserId
            : (int?)null;
        var result = await _service.Donate(userId, fundraiserId ,uniqueCode , dto, ct);
        return Ok(result);
    }
    
    [HttpPost("{fundraiserId}/donate")]
    public async Task<IActionResult> DirectDonate(int fundraiserId, [FromBody] CreateDonationDto dto, CancellationToken ct = default)
    {
        var userId = User.Identity?.IsAuthenticated == true
            ? CurrentUserId
            : (int?)null;
        var result = await _service.DirectDonate(userId, fundraiserId, dto, ct);
        return Ok(result);
    }
}