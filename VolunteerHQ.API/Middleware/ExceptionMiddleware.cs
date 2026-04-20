using VolunteerHQ.Core.Exceptions;

namespace VolunteerHQ.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }

        catch (Exception ex)
        {
            var statusCode = ex switch
            {
                UnauthorizedException => 401,
                NotFoundException => 404,
                ConflictEmailException => 409,
                NotEnoughRightsException => 403,
                _ => 500
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            var response = new { error = ex.Message };
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}