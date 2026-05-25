using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using VolunteerHQ.Core.Exceptions;

namespace VolunteerHQ.API.Controllers;

[ApiController]
public class BaseController : ControllerBase
{
    protected int CurrentUserId
    {
        get
        {
            var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(raw, out var id))
                throw new UnauthorizedException("Invalid or missing authentication token");

            return id;
        }
    }
}
