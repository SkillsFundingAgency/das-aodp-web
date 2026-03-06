using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Application.Commands.Qualifications
{
    [ExcludeFromCodeCoverage]
    public class BulkUpdateQualificationStatusCommandResponse
    {
        public Guid ProcessStatusId { get; init; }
        public string ProcessStatusName { get; init; } = default!;
        public int RequestedCount { get; init; }
        public int UpdatedCount { get; init; }
        public int ErrorCount { get; init; }
        public IReadOnlyCollection<BulkUpdateQualificationsErrorDto> Errors { get; init; }
            = Array.Empty<BulkUpdateQualificationsErrorDto>();
    }

    [ExcludeFromCodeCoverage]
    public class BulkUpdateQualificationsErrorDto
    {
        public Guid QualificationId { get; init; }
        public string Qan { get; init; } = default!;
        public string Title { get; init; } = default!;
        public BulkUpdateQualificationsErrorType ErrorType { get; init; }
    }

    public enum BulkUpdateQualificationsErrorType
    {
        Missing = 1,
        StatusUpdateFailed = 2,
        HistoryFailed = 3
    }
}
