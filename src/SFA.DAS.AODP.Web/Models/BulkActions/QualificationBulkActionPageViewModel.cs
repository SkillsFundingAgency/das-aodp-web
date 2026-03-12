using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.AODP.Web.Validators.Attributes;
using SFA.DAS.AODP.Web.Validators.Messages;
using SFA.DAS.AODP.Web.Validators.Patterns;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Models.BulkActions
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// Base view model for list pages that support selecting rows and applying a bulk action.
    /// </summary>
    public abstract class QualificationsBulkActionPageViewModel
    {

        /// <summary>
        /// The ids of qualifications selected in the list/table.
        /// </summary>
        [MinLength(1, ErrorMessage = ValidationMessages.QualificationsBulkAction.NoQualificationsSelected)]
        public List<Guid> SelectedQualificationIds { get; set; } = new();

        /// <summary>
        /// User-submitted bulk action values (posted from the form).
        /// </summary>
        public QualificationsBulkActionInputViewModel BulkAction { get; set; } = new();

        /// <summary>
        /// Options for the qualifications bulk action status dropdown.
        /// </summary>
        public List<SelectListItem> BulkActionStatusOptions { get; set; } = new();

        public void SetBulkActionStatusOptions(
            IEnumerable<(Guid Id, string Name)> statuses)
        {
            BulkActionStatusOptions = statuses
                .Where(s => BulkActionQualifications.AllowedStatuses.Contains(s.Name ?? ""))
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name ?? ""
                })
                .ToList();
        }

        /// <summary>
        /// Posted bulk action inputs (status + optional comment).
        /// </summary>
        public class QualificationsBulkActionInputViewModel
        {
            [Display(Name = "Status")]
            [Required(ErrorMessage = ValidationMessages.QualificationsBulkAction.StatusRequired)]
            public Guid? ProcessStatusId { get; set; }

            [AllowedCharacters(TextCharacterProfile.FreeText)]
            public string? Comment { get; set; }
        }
    }
}