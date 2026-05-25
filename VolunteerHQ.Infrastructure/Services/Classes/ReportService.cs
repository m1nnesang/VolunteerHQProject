using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.ReportDTOs;
using VolunteerHQ.Core.Enums;
using VolunteerHQ.Core.Exceptions;
using VolunteerHQ.Core.Models;
using VolunteerHQ.Infrastructure.Data;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.Infrastructure.Services.Classes;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    private readonly ValidatorService _vs;
    private readonly IAuditLogService _als;

    public ReportService(AppDbContext db, ValidatorService vs, IAuditLogService als)
    {
        _db = db;
        _vs = vs;
        _als = als;
    }

    public async Task<ReportResponseDto> CreateReport(int reporterId, CreateReportDto dto, CancellationToken ct = default)
    {
        await _vs.GetUserByIdOrThrow(reporterId, ct);
        await _vs.GetUserByIdOrThrow(dto.ReportedId, ct);

        if (reporterId == dto.ReportedId)
            throw new ConflictException("You cannot report yourself");

        var report = new ReportModel
        {
            ReporterId = reporterId,
            ReportedId = dto.ReportedId,
            Reason = dto.Reason,
            Category = dto.Category,
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        await _db.AddAsync(report, ct);
        await _db.SaveChangesAsync(ct);

        return ToDto(report);
    }

    public async Task<ReportResponseDto> GetReport(int userId, int reportId, CancellationToken ct = default)
    {
        var user = await _vs.GetUserByIdOrThrow(userId, ct);
        var report = await _vs.GetReportOrThrow(reportId, ct);

        if (report.ReporterId != userId && user.Role != UserRoles.Admin)
            throw new NotEnoughRightsException("You don't have rights for this operation");

        return ToDto(report);
    }

    public async Task<PagedResponseDto<ReportResponseDto>> GetAllReports(int userId, PaginationDto pagination, CancellationToken ct = default)
    {
        await _vs.AdminOrThrow(userId, ct);

        var total = await _db.Reports.CountAsync(ct);

        var items = await _db.Reports
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(r => new ReportResponseDto(r.Id, r.ReporterId, r.ReportedId, r.Reason, r.Category, r.Status, r.CreatedAt, r.ReviewedAt, r.AdminComment))
            .ToListAsync(ct);

        return new PagedResponseDto<ReportResponseDto>(items, total, pagination.Page, pagination.PageSize);
    }

    public async Task<ReportResponseDto> ReviewReport(int userId, int reportId, ReviewReportDto dto, CancellationToken ct = default)
    {
        await _vs.AdminOrThrow(userId, ct);

        var report = await _vs.GetReportOrThrow(reportId, ct);

        report.Status = dto.Status;
        report.AdminComment = dto.AdminComment;
        report.ReviewedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _als.Log(userId, dto.Status.ToString(), "Report", report.Id,
            $"Report {report.Id} marked as {dto.Status} by admin {userId}", ct);

        return ToDto(report);
    }

    private static ReportResponseDto ToDto(ReportModel report) =>
        new(report.Id, report.ReporterId, report.ReportedId, report.Reason,
            report.Category, report.Status, report.CreatedAt, report.ReviewedAt, report.AdminComment);
}
