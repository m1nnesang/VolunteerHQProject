using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.DTOs.MembershipDTOs;

public record UpdateMemberRoleDto(int UserId, OrganizationMemberRole newRole);
