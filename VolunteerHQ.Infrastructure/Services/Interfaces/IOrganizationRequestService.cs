using VolunteerHQ.Core.DTOs.OrganizationRequestDTOs;
using VolunteerHQ.Core.DTOs.Common;

namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface IOrganizationRequestService 
{
    Task<OrganizationRequestResponseDto> CreateRequest(int userId, CreateOrganizationRequestDto dto, CancellationToken ct = default);
    Task<OrganizationRequestResponseDto> GetCreateRequest(int userId, int requestId, CancellationToken ct = default);

    Task<PagedResponseDto<OrganizationRequestResponseDto>> GetAllRequests(int userId, int page = 1, int pageSize = 20,
        CancellationToken ct = default);
    Task<OrganizationRequestResponseDto> ReviewOrganizationRequest(int userId, int requestId, ReviewOrganizationRequestDto dto, CancellationToken ct = default);
}
