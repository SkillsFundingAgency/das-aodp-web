using SFA.DAS.AODP.Web.Validators.Attributes;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview
{
    public class UpdateQanViewModel
    {
        [QualificationNumber]
        public string? Qan { get; set; }
        public Guid ApplicationReviewId { get; set; }
    }
}
