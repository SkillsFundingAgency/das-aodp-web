using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public class UploadBespokeQualificationCandidatesViewModel //: IValidatableObject
    {
        [Required(ErrorMessage = "You must select a CSV file")]
        public IFormFile File { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
    }
}