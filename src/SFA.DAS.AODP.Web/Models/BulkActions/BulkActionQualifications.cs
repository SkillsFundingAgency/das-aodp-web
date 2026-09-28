using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Web.Enums;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Models.BulkActions
{
    [ExcludeFromCodeCoverage]
    public static class BulkActionQualifications
    {
        public const string SuccessKey = "Bulk:Qualifications:Success";
        public const string SuccessMessage = "Actions have been applied to the selected qualifications";
        public const string Errors = "Bulk:Qualifications:Errors";

        public static readonly string[] AllowedStatuses =
        {
            ProcessStatusLookup.DecisionRequired.Name,
            ProcessStatusLookup.NoActionRequired.Name,
            ProcessStatusLookup.OnHold.Name,
        };
    }

    public static class BulkActionApplications
    {
        public const string BulkActionSuccessKey = "Bulk:Applications:Success";
        public const string BulkActionSuccessMessage = "Actions have been applied to the selected applications";

        public const string SaveReviewersSuccessKey = "Bulk:Applications:SaveReviewers:Success";
        public const string SaveReviewersSuccessMessage = "Reviewers have been saved for the selected applications";

        public const string FailedKey = "Bulk:Applications:Failed";
        public const string FailedMessage = "Applications could not be updated. Please try again later.";
        public const string Errors = "Bulk:Applications:Errors";

        public static readonly string[] AllowedActions =
        {
            MessageType.ApplicationSharedWithOfqual.ToString(),
            MessageType.ApplicationSharedWithSkillsEngland.ToString(),
            MessageType.UnlockApplication.ToString(),
        };
    }
}
