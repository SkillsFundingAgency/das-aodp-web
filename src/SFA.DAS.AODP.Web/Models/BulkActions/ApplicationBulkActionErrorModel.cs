using SFA.DAS.AODP.Application.Commands.Review;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Models.BulkActions
{
    [ExcludeFromCodeCoverage]
    public class ApplicationBulkActionErrorModel
    {
        public List<ApplicationBulkActionErrorItemViewModel> Failed { get; set; } = new();
        public string BackLinkText { get; set; } = string.Empty;
        public string BackLinkUrl { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class ApplicationBulkActionErrorItemViewModel
    {
        public int ReferenceNumber { get; set; }
        public string ApplicationReference =>
            ReferenceNumber > 0
                ? ReferenceNumber.ToString("D6")
                : string.Empty;
        public string? Qan { get; set; }
        public string? Title { get; set; }
        public string? AwardingOrganisation { get; set; }
        public string DisplayName => string.IsNullOrWhiteSpace(Title)
                ? "Unknown application"
                : Title!;
        public string FailureReason { get; set; } = string.Empty;
    }
}
