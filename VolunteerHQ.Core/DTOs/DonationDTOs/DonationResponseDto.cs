namespace VolunteerHQ.Core.DTOs.DonationDTOs;

public record DonationResponseDto(int Id , int? UserId,  decimal Amount , DateTime CreatedAt);