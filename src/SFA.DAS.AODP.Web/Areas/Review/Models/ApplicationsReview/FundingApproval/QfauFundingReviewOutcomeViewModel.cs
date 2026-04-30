using SFA.DAS.AODP.Web.Validators.Attributes;
using SFA.DAS.AODP.Web.Validators.Patterns;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview.FundingApproval
{
    public class QfauFundingReviewOutcomeViewModel
    {
        public Guid ApplicationReviewId { get; set; }

        [AllowedCharacters(TextCharacterProfile.FreeText)]
        public string? Comments { get; set; }

        [Required(ErrorMessage = "You must select an outcome for funding in order to proceed.")]
        public bool? Approved { get; set; }
        public bool? NewDecision { get; set; }
    }
}
