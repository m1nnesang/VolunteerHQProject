using System.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using VolunteerHQ.Core.DTOs.AuditLogDTOs;
using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.Models;
using VolunteerHQ.Infrastructure.Data;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.Infrastructure.Services.Classes;

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _db;
    private readonly ValidatorService _vs;

    public AuditLogService(AppDbContext db, ValidatorService vs)
    {
        _db = db;
        _vs = vs;
    }

    public async Task Log(int userId, string action, string entityType, int entityId, string details,
        CancellationToken ct = default)
    {
        var log = new AuditLogModel()
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            CreatedAt = DateTime.UtcNow
        };

        await _db.AddAsync(log, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PagedResponseDto<AuditLogResponseDto>> GetLogs(int userId, PaginationDto pagination,
        CancellationToken ct = default)
    {
        await _vs.AdminOrThrow(userId, ct);

        var total = await _db.AuditLogs.CountAsync(ct);

        var items = await _db.AuditLogs
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(l =>
                new AuditLogResponseDto(l.Id, l.UserId, l.Action, l.EntityType, l.EntityId, l.Details, l.CreatedAt))
            .ToListAsync(ct);

        return new PagedResponseDto<AuditLogResponseDto>(items, total, pagination.Page, pagination.PageSize);
    }
}