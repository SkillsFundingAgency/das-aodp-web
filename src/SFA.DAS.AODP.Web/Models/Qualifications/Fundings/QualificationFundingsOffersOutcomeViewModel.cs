using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Qualifications.Fundings
{
    public class QualificationFundingsOffersOutcomeViewModel
    {
        public Guid QualificationVersionId { get; set; }
        public string? Comments { get; set; }

        [Required]
        public bool? Approved { get; set; }
        public bool? NewDecision { get; set; }
    }
}
