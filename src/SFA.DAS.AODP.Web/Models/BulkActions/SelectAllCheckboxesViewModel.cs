using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Models.BulkActions
{
    [ExcludeFromCodeCoverage]
    public class SelectAllCheckboxesViewModel
    {
        public string? AnchorId { get; set; } = "bulk-action-select-all";
        public string? Action { get; set; } = "Index";
        public string? Controller { get; set; }
        public string? Area { get; set; } = "Review";

    }
}
