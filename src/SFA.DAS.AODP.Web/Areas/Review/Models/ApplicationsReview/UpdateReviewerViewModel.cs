namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview
{
    public class UpdateReviewerViewModel
    {
        public string ReviewerFieldName { get; set; }
        public string? ReviewerValue { get; set; }
        public Guid ApplicationReviewId { get; set; }
        public Guid ApplicationId { get; set; }
    }
}
