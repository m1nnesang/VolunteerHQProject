using VolunteerHQ.Core.DTOs.MembershipDTOs;

namespace VolunteerHQ.Core.DTOs.OrganizationDTOs;

public record OrganizationResponseDto(int Id, string Name, string City, string Description, DateTime CreatedAt, List<MembershipResponseDto> Members);