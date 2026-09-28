using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Consent
{
    public class DataConsentViewModel
    {
        [Display(Name = "I have read and understood the privacy notice")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Select that you have read and understood the privacy notice")]
        public bool HasAccepted { get; set; }
    }
}