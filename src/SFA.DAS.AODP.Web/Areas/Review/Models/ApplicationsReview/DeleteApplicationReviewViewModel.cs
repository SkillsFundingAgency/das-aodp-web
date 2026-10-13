namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview
{
    public class DeleteApplicationReviewViewModel
    {
        public Guid ApplicationReviewId { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public int ApplicationReference { get; set; }
    }
}
