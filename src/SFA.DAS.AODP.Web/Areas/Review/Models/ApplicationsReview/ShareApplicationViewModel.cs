using SFA.DAS.AODP.Models.Users;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview
{
    public class ShareApplicationViewModel
    {
        public UserType UserType { get; set; }
        public Guid ApplicationReviewId { get; set; }
        public bool Share { get; set; }
    }

}
