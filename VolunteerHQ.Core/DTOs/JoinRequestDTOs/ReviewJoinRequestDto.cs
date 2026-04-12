using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.DTOs.JoinRequestDTOs;

public record ReviewJoinRequestDto(RequestStatus Status , string? AdminComment);