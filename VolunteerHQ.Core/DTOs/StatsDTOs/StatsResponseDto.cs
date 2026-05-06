namespace VolunteerHQ.Core.DTOs.StatsDTOs;

public record StatsResponseDto(
    int ActiveFundraisers,
    int CompletedFundraisers,
    decimal TotalDonated,
    int UniqueDonors,
    int OrganizationsCount,
    int MilitaryUnitsCount);
