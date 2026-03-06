using SFA.DAS.AODP.Web.Validators.Messages;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.BulkActions
{
    public class ApplicationsBulkActionPostModel : IValidatableObject
    {
        [MinLength(1, ErrorMessage = ValidationMessages.ApplicationsBulkAction.NoApplicationsSelected)]
        public List<Guid> SelectedApplicationIds { get; set; } = new();
        public ApplicationsBulkActionInputViewModel BulkActionInputViewModel { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SelectedApplicationIds.Count == 0)
            {
                yield return new ValidationResult(
                    ValidationMessages.ApplicationsBulkAction.NoApplicationsSelected,
                    new[] { nameof(SelectedApplicationIds) });
            }
        }
    }
}
