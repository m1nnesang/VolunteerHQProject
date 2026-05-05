using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.DTOs.SubscriptionDTOs;

public record CreateSubscriptionDto(SubscriptionTargetType Target, int TargetId);