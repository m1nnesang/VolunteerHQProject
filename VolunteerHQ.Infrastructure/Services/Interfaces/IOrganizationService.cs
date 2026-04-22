using VolunteerHQ.Core.DTOs.MembershipDTOs;
using VolunteerHQ.Core.DTOs.OrganizationDTOs;

namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface IOrganizationService
{
     Task<OrganizationResponseDto> GetOrganization(int orgId, CancellationToken ct = default);
      Task<List<MembershipResponseDto>> GetOrganizationMembers(int orgId, CancellationToken ct = default);
     Task RemoveMember(int orgId, int requesterId, int targetId, CancellationToken ct = default);
      Task UpdateMemberRole(int orgId, int requesterId, int targetId, UpdateMemberRoleDto dto, 
          CancellationToken ct = default);
}