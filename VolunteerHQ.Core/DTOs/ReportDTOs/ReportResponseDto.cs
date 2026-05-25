using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.DTOs.ReportDTOs;

public record ReportResponseDto(
    int Id,
    int? ReporterId,
    int? ReportedId,
    string Reason,
    ReportCategory Category,
    ReportStatus Status,
    DateTime CreatedAt,
    DateTime? ReviewedAt,
    string? AdminComment);
