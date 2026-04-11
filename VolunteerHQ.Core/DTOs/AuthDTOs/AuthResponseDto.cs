using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.DTOs.AuthDTOs;

public record AuthResponseDto (int UserId , UserRoles Role , string Token );