using VolunteerHQ.Core.Enums;

namespace VolunteerHQ.Core.DTOs.SubscriptionDTOs;

public record SubscriptionResponseDto(int Id , int? UserId, SubscriptionTargetType Target, int TargetId , DateTime SubscribedAt);