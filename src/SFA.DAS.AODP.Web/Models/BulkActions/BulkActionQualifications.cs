using SFA.DAS.AODP.Models.Qualifications;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Models.BulkActions
{
    [ExcludeFromCodeCoverage]
    public static class BulkActionQualifications
    {
        public const string SuccessKey = "Bulk:Qualifications:Success";
        public const string SuccessMessage = "Actions have been applied to the selected qualifications.";
        public const string FailedKey = "Bulk:Qualifications:Failed";
        public const string FailedMessage = "Qualifications could not be updated. Please try again later.";

        public static readonly string[] AllowedStatuses =
        {
            ProcessStatus.DecisionRequired,
            ProcessStatus.NoActionRequired,
            ProcessStatus.OnHold
        };
    }
}
