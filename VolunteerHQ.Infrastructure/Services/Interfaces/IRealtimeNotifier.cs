namespace VolunteerHQ.Infrastructure.Services.Interfaces;

public interface IRealtimeNotifier
{
    Task SendMessage(int receiverUserId, object message, CancellationToken ct = default);
    Task SendNotification(int userId, object notification, CancellationToken ct = default);
}
