using SFA.DAS.AODP.Application.Exceptions;

namespace SFA.DAS.AODP.Web.Middleware;

/// <summary>
/// Provides a better mechanism for logging issues as proper exceptions and not burying them within traces.
/// </summary>
/// <param name="next">The next delegate in the pipeline.</param>
/// <param name="logger">The logger to use for logging exceptions.</param>
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
                ex,
                "Unhandled exception from MediatR request {MediatrRequestName} while handling {HttpMethod} {Path}.",
                ex.RequestName,
                context.Request.Method,
                context.Request.Path.Value);

            throw;
        }
    }
}
