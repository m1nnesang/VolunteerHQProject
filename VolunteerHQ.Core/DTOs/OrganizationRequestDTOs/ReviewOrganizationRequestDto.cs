using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.DTOs.OrganizationRequestDTOs;

public record ReviewOrganizationRequestDto(RequestStatus Status , string? AdminComment);