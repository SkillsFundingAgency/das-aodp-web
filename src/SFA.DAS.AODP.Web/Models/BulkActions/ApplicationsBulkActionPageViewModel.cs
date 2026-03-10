using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.AODP.Application.Commands.Review;
using SFA.DAS.AODP.Web.Constants;
using SFA.DAS.AODP.Web.Validators.Messages;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.BulkActions
{
    public abstract class ApplicationsBulkActionPageViewModel 
    {
        [MinLength(1, ErrorMessage = ValidationMessages.ApplicationsBulkAction.NoApplicationsSelected)]
        public List<Guid> SelectedApplicationReviewIds { get; set; } = new();

        public List<SelectListItem> ReviewerOptions { get; set; } = new();

        public List<SelectListItem> BulkActionOptions { get; set; } = new();

        public ApplicationsBulkActionInputViewModel BulkActionInputViewModel { get; set; } = new();

        public ApplicationsBulkActionResult ApplicationsBulkActionResult { get; set; } = new();
    }

    public class ApplicationsBulkActionInputViewModel : IValidatableObject
    {
        [Display(Name = "Reviewer 1")]
        public string? Reviewer1 { get; set; }

        [Display(Name = "Reviewer 2")]
        public string? Reviewer2 { get; set; }

        [Display(Name = "Bulk action")]
        public BulkApplicationActionType? BulkActionType { get; set; }

        public SubmitAction SubmitAction { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SubmitAction == SubmitAction.Message && BulkActionType is null)
            {
                yield return new ValidationResult(
                    ValidationMessages.ApplicationsBulkAction.NoActionSelected,
                    [nameof(BulkActionType)]);
            }

            if (SubmitAction == SubmitAction.Assign)
            {
                if(string.IsNullOrEmpty(Reviewer1) && string.IsNullOrEmpty(Reviewer2))
                {
                    yield return new ValidationResult(
                    ValidationMessages.ApplicationsBulkAction.NoReviewerSelected,
                    new[] { nameof(Reviewer1) });
                }

                if (!string.IsNullOrEmpty(Reviewer1) &&
                    !string.IsNullOrEmpty(Reviewer2) &&
                    Reviewer1 == Reviewer2 &&
                    Reviewer1 != ReviewerDropdown.UnassignedValue
                    )
                {
                    yield return new ValidationResult(
                        ValidationMessages.Reviewer1Reviewer2Conflict,
                        [nameof(Reviewer1)]);
                }
            }
        }

    }   

    public enum SubmitAction
    {
        Assign,
        Message
    }

    public class ApplicationsBulkActionResult
    {
        public int TotalApplications { get; set; }
        public int SuccessfulApplications { get; set; }
        public int FailedApplications { get; set; }
        public List<string> ErrorMessages { get; set; } = new();
    }
}
