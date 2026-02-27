using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Models.BulkActions
{
    [ExcludeFromCodeCoverage]
    public class SelectAllQualificationsViewModel
    {
        public int CurrentPage { get; set; }
        public int RecordsPerPage { get; set; }
        public string? Name { get; set; }
        public string? Organisation { get; set; }
        public string? Qan { get; set; }
        public IEnumerable<Guid>? ProcessStatusIds { get; set; }
        public string? AnchorId { get; set; } = "bulk-action-select-all";
        public string? Action { get; set; } = "Index";
        public string? Controller { get; set; } 
        public string? Area { get; set; } = "Review";
    }
}
