using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.DTOs.MembershipDTOs;

public record MembershipResponseDto(int Id , int UserId , int OrganizationId , OrganizationMemberRole Role, DateTime JoinedAt, string? FirstName, string? SecondName);