using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Qualifications.Fundings
{
    public class QualificationFundingsOffersOutcomeViewModel
    {
        public Guid QualificationVersionId { get; set; }
        public Guid QualificationId { get; set; }
        public string QualificationReference { get; set; }
        public string Mode { get; set; }
        public string? Comments { get; set; }

        [Required(ErrorMessage = "You must select an outcome for funding in order to proceed.")]
        public bool? Approved { get; set; }
    }
}
