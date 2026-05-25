using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace VolunteerHQ.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
}
