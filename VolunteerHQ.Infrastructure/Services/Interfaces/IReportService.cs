using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.ReportDTOs;

namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface IReportService
{
    Task<ReportResponseDto> CreateReport(int reporterId, CreateReportDto dto, CancellationToken ct = default);
    Task<ReportResponseDto> GetReport(int userId, int reportId, CancellationToken ct = default);
    Task<PagedResponseDto<ReportResponseDto>> GetAllReports(int userId, PaginationDto pagination, CancellationToken ct = default);
    Task<ReportResponseDto> ReviewReport(int userId, int reportId, ReviewReportDto dto, CancellationToken ct = default);
}
