using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Application.Commands.Review;

public class BulkApplicationActionCommandResponse
{
    public int ErrorCount { get; set; }
}

[ExcludeFromCodeCoverage]
public class BulkApplicationActionErrorDto
{
    public BulkApplicationActionErrorType ErrorType { get; init; }
}

public enum BulkApplicationActionErrorType
{
    Error1 = 1,
    Error2 = 2,
    Error3 = 3
}