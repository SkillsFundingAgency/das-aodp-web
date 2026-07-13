using SFA.DAS.AODP.Application.Exceptions;

namespace SFA.DAS.AODP.Application.Behaviours;

public sealed class MediatrExceptionHandlingBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (MediatrRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MediatrRequestException(typeof(TRequest).Name, ex);
        }
    }
}