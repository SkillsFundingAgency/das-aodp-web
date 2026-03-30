using SFA.DAS.AODP.Models.Qualifications;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview.FundingApproval
{
    public static class FundingDecisionMessages
    {
        public const string NotApprovedWithOffers =
            "The application status is 'Not approved', but there are approved offers for the application. Please review the funding decision.";

        public const string InvalidStatus =
            "The application status is not valid. Please either approve or reject the application for funding before informing the AO.";

        public const string MissingQualification =
            "The application's QAN does not match any regulated qualification.";

        public static string InvalidQualificationStatus(string status) =>
            $"The Ofqual qualification status is not valid to confirm the decision. The current qualification status is: {status}.";
    }
}
