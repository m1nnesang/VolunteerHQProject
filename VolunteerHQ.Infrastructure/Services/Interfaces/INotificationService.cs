using VolunteerHQ.Core.DTOs.Common;
using VolunteerHQ.Core.DTOs.NotificationDTOs;

namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface INotificationService
{
    Task<PagedResponseDto<NotificationResponseDto>> GetNotifications(int userId, PaginationDto pagination, CancellationToken ct = default);
    Task MarkAsRead(int userId, int notificationId, CancellationToken ct = default);
    Task SendNotification(int userId, string text, string link, CancellationToken ct = default);
}