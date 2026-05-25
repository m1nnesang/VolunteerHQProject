using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.DTOs.ReportDTOs;

public record ReviewReportDto(ReportStatus Status, string? AdminComment);
