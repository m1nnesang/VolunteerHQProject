using VolunteerHQ.Core.DTOs.OrganizationRequestDTOs;

namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface IOrganizationRequestService 
{
    Task<OrganizationRequestResponseDto> CreateRequest(int userId, CreateOrganizationRequestDto dto, CancellationToken ct = default);
    Task<OrganizationRequestResponseDto> GetCreateRequest(int userId, int requestId, CancellationToken ct = default);
    Task<List<OrganizationRequestResponseDto>> GetAllRequests(int userId, CancellationToken ct = default);
    Task<OrganizationRequestResponseDto> ReviewOrganizationRequest(int userId, int requestId, ReviewOrganizationRequestDto dto, CancellationToken ct = default);
}
