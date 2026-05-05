using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.SubscriptionDTOs;

namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface ISubscriptionService
{
    Task<SubscriptionResponseDto> Subscribe(int userId, CreateSubscriptionDto dto, CancellationToken ct);
    Task Unsubscribe(int userId, int subscriptionId, CancellationToken ct);
    Task<PagedResponseDto<SubscriptionResponseDto>> GetSubscriptions(int userId, PaginationDto pagination, CancellationToken ct);
}