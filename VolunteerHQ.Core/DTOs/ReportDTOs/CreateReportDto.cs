using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.DTOs.ReportDTOs;

public record CreateReportDto(int ReportedId, string Reason, ReportCategory Category);
