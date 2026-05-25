namespace VolunteerHQ.Core.DTOs.AuditLogDTOs;

public record AuditLogResponseDto(
    int Id,
    int UserId,
    string Action,
    string EntityType,
    int EntityId,
    string Details,
    DateTime CreatedAt);
