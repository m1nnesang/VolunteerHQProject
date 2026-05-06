namespace VolunteerHQ.Core.DTOs.StatsDTOs;

public record FundraiserStatsResponseDto(
    int FundraiserId,
    decimal CurrentProgress,
    int DonorsCount,
    List<TopDonorDto> TopDonors);
