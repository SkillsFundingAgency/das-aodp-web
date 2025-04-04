using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview.FundingApproval
{
    public class QfauFundingReviewOutcomeViewModel
    {
        public Guid ApplicationReviewId { get; set; }
        public string? Comments { get; set; }

        [Required(ErrorMessage = "You must select an outcome for funding in order to proceed.")]
        public bool? Approved { get; set; }
        public bool? NewDecision { get; set; }
    }
}
