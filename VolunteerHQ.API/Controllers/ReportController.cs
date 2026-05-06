using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.ReportDTOs;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.API.Controllers;

[Route("api/report")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;
    
    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportDto request , CancellationToken ct = default)
    {
        var reporterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        
        var result = await _reportService.CreateReport(reporterId, request, ct);
        return Ok(result);
    }

    [HttpGet("{reportId}")]
    [Authorize]
    public async Task<IActionResult> GetReport(int reportId, CancellationToken ct = default)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        
        var result = await _reportService.GetReport(userId, reportId, ct);
        return Ok(result);
    }

    [HttpGet("all")]
    [Authorize]
    public async Task<IActionResult> GetAllReports([FromQuery] PaginationDto pagination, CancellationToken ct = default)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        
        var result = await _reportService.GetAllReports(userId, pagination, ct);
        return Ok(result);
    }

    [HttpPut("{reportId}/review")]
    [Authorize]
    public async Task<IActionResult> ReviewReport(int reportId, [FromBody] ReviewReportDto dto, CancellationToken ct = default)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _reportService.ReviewReport(userId, reportId, dto, ct);
        return Ok(result);
    }
}