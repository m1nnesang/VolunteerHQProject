using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.DTOs.MembershipDTOs;

public record MembershipResponseDto(int Id , int OrganizationId , OrganizationMemberRole Role, DateTime JoinedAt);