using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.DTOs.JoinRequestDTOs;

public record JoinRequestResponseDto(int Id , int UserId , int OrganizationId , RequestStatus Status , DateTime CreatedAt , DateTime? ReviewedAt , int? ReviewedByUserId);