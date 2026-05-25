using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.DTOs.OrganizationRequestDTOs;

public record OrganizationRequestResponseDto(int Id , int UserId ,string Bio , string Experience , string Skills , string CvFilePath , string ProposedName , string City , string Description , RequestStatus Status , DateTime? ReviewedAt , int? ReviewedByUserId , string? AdminComment);