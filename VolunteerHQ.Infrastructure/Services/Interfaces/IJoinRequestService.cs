using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.JoinRequestDTOs;

 namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface IJoinRequestService
{
    Task<JoinRequestResponseDto> CreateJoinRequest(int userId, int orgId, CreateJoinRequestDto dto, CancellationToken ct = default);
    Task<JoinRequestResponseDto> GetJoinRequest(int joinRequestId, int userId, int orgId, CancellationToken ct = default);

    Task<PagedResponseDto<JoinRequestResponseDto>> GetAllJoinRequests(int orgId, int page = 1, int pageSize = 20,
        CancellationToken ct = default);
    Task<JoinRequestResponseDto> ReviewJoinRequest(ReviewJoinRequestDto dto, int reviewerId, int orgId, int requestId, CancellationToken ct = default);
}
