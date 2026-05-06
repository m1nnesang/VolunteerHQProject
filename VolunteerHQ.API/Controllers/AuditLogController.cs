using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.API.Controllers;

[ApiController]
[Route("api/auditlog")]
public class AuditLogController : BaseController
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetLogs([FromQuery] PaginationDto pagination, CancellationToken ct = default)
    {
        var userId = CurrentUserId;
        var result = await _auditLogService.GetLogs(userId, pagination, ct);
        return Ok(result);
    }
}
