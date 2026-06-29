using SFA.DAS.AODP.Application.Exceptions;

namespace SFA.DAS.AODP.Web.Middleware;

public sealed class MediatrExceptionLoggingMiddleware(RequestDelegate next, ILogger<MediatrExceptionLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (MediatrRequestException ex)
        {
            logger.LogError(
                ex.InnerException ?? ex,
                "Unhandled exception from MediatR request {MediatrRequestName} while handling {HttpMethod} {Path}.",
                ex.RequestName,
                context.Request.Method,
                context.Request.Path.Value);

            throw;
        }
    }
}
