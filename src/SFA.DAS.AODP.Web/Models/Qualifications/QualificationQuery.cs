using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Models.Qualifications
{
    [ExcludeFromCodeCoverage]
    public class QualificationQuery
    {
        public List<Guid>? ProcessStatusIds { get; init; }
        public int PageNumber { get; set; } = 1;
        public int RecordsPerPage { get; init; } = 10;
        public string? Name { get; init; } = string.Empty;
        public string? Organisation { get; init; } = string.Empty;
        public string? Qan { get; init; } = string.Empty;
    }
}
