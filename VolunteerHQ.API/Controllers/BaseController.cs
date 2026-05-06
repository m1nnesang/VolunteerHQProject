using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace VolunteerHQ.API.Controllers;

[ApiController]
public class BaseController : ControllerBase
{
    protected int CurrentUserId =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
}
