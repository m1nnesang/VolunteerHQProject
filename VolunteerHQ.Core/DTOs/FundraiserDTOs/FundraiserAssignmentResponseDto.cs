using VolunteerHQ.Core.Models;

namespace VolunteerHQ.Core.DTOs.FundraiserDTOs;

public record FundraiserAssignmentResponseDto(int Id , decimal AmountRaised, int OrganizationId , string UniqueId, DateTime TakenAt);