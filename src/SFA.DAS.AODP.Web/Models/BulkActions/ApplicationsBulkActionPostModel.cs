using SFA.DAS.AODP.Web.Validators.Messages;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Models.BulkActions
{
    [ExcludeFromCodeCoverage]
    public class ApplicationsBulkActionPostModel : IValidatableObject
    {
        [MinLength(1, ErrorMessage = ValidationMessages.ApplicationsBulkAction.NoApplicationsSelected)]
        public List<Guid> SelectedApplicationReviewIds { get; set; } = new();
        public ApplicationsBulkActionInputViewModel BulkActionInputViewModel { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SelectedApplicationReviewIds.Count == 0)
            {
                yield return new ValidationResult(
                    ValidationMessages.ApplicationsBulkAction.NoApplicationsSelected,
                    new[] { nameof(SelectedApplicationReviewIds) });
            }
        }
    }
}
