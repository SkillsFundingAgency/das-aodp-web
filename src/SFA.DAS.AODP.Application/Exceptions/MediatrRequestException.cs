namespace SFA.DAS.AODP.Application.Exceptions;

public sealed class MediatrRequestException : Exception
{
    public MediatrRequestException(string requestName, Exception innerException)
        : base($"Unhandled exception while handling MediatR request {requestName}.", innerException)
    {
        RequestName = requestName;
    }

    public string RequestName { get; }
}
