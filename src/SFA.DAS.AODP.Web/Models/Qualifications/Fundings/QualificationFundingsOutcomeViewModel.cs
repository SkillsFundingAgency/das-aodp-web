using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Qualifications.Fundings
{
    public class QualificationFundingsOutcomeViewModel
    {
        public Guid ApplicationReviewId { get; set; }
        public string? Comments { get; set; }

        [Required]
        public bool? Approved { get; set; }
        public bool? NewDecision { get; set; }
    }
}
