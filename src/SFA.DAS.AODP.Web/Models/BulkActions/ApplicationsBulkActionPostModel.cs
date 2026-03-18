using SFA.DAS.AODP.Web.Validators.Messages;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Models.BulkActions
{
    [ExcludeFromCodeCoverage]
    public class ApplicationsBulkActionPostModel
    {
        public List<Guid> SelectedApplicationReviewIds { get; set; } = new();
        public ApplicationsBulkActionInputViewModel BulkActionInputViewModel { get; set; } = new();
    }
}
