using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.MembershipDTOs;
using VolunteerHQ.Core.DTOs.OrganizationDTOs;

namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface IOrganizationService
{
     Task<OrganizationResponseDto> GetOrganization(int orgId, CancellationToken ct = default);

     Task<PagedResponseDto<MembershipResponseDto>> GetOrganizationMembers(int orgId, int page = 1, int pageSize = 20,
         CancellationToken ct = default);
     Task RemoveMember(int orgId, int requesterId, int targetId, CancellationToken ct = default);
      Task UpdateMemberRole(int orgId, int requesterId, int targetId, UpdateMemberRoleDto dto, 
          CancellationToken ct = default);
}