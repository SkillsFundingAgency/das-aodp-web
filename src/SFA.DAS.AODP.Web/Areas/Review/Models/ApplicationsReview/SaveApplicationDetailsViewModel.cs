using SFA.DAS.AODP.Web.Validators.Attributes;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview
{
    public class SaveApplicationDetailsViewModel
    {
        [QualificationNumber]
        public string? Qan { get; set; }

        public string? Reviewer1 { get; set; }
        public string? Reviewer2 { get; set; }

        public Guid ApplicationReviewId { get; set; }
        public Guid ApplicationId { get; set; }
    }
}
