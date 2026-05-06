using VolunteerHQ.Core.DTOs.AuditLogDTOs;
using VolunteerHQ.Core.DTOs.Common;

namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface IAuditLogService
{
    Task Log(int userId, string action, string entityType, int entityId, string details, CancellationToken ct = default);
    Task<PagedResponseDto<AuditLogResponseDto>> GetLogs(int userId, PaginationDto pagination, CancellationToken ct = default);
}
