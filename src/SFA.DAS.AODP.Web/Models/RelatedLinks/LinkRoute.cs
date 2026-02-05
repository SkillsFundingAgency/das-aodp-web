namespace SFA.DAS.AODP.Web.Models.RelatedLinks
{
    public static class LinkRoute
    {
        public static class Areas
        {
            public const string Apply = "Apply";
            public const string Review = "Review";
        }

        public static class Controllers
        {
            public const string Applications = "Applications";
            public const string ApplicationsReview = "ApplicationsReview";
            public const string ApplicationMessages = "ApplicationMessages";
        }

        public static class Actions
        {
            // Apply
            public static string ViewApplication => nameof(Web.Areas.Apply.Controllers.ApplicationsController.ViewApplication);
            public static string ApplyMessages => nameof(Web.Areas.Apply.Controllers.ApplicationMessagesController.ApplicationMessages);

            // Review
            public static string ReviewMessages => nameof(Web.Areas.Review.Controllers.ApplicationMessagesController.ApplicationMessages);
            public static string ViewApplicationReadOnlyDetails => nameof(Web.Areas.Review.Controllers.ApplicationsReviewController.ViewApplicationReadOnlyDetails);
            public static string ViewApplicationReview => nameof(Web.Areas.Review.Controllers.ApplicationsReviewController.ViewApplication);
        }
    }
}
