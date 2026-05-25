namespace VolunteerHQ.Core.DTOs.UserDTOs;

public record UserStatsDto(
    decimal TotalDonated,
    int DonationsCount,
    int FundraisersSupported
);
