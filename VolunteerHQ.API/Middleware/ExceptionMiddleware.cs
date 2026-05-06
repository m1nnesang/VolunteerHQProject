using VolunteerHQ.Core.Exceptions;

namespace VolunteerHQ.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
                ConflictException => 409,
                _ => 500
            };

            if (statusCode == 500)
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                    context.Request.Method, context.Request.Path);
            else
                _logger.LogWarning(ex, "{ExceptionType} on {Method} {Path}",
                    ex.GetType().Name, context.Request.Method, context.Request.Path);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            var response = new { error = ex.Message };
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}