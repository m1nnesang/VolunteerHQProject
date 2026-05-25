using Microsoft.AspNetCore.SignalR;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.API.Hubs;

public class SignalRNotifier : IRealtimeNotifier
{
    private readonly IHubContext<ChatHub> _hub;

    public SignalRNotifier(IHubContext<ChatHub> hub)
    {
        _hub = hub;
    }

    public Task SendMessage(int receiverUserId, object message, CancellationToken ct = default) =>
        _hub.Clients.User(receiverUserId.ToString())
            .SendAsync("ReceiveMessage", message, ct);

    public Task SendNotification(int userId, object notification, CancellationToken ct = default) =>
        _hub.Clients.User(userId.ToString())
            .SendAsync("ReceiveNotification", notification, ct);
}
