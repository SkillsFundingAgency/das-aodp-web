using SFA.DAS.AODP.Web.Validators.Messages;
using System.ComponentModel.DataAnnotations;
using static SFA.DAS.AODP.Web.Models.BulkActions.QualificationsBulkActionPageViewModel;

namespace SFA.DAS.AODP.Web.Models.BulkActions
{
    public class QualificationBulkActionPostModel
    {
        [MinLength(1, ErrorMessage = ValidationMessages.QualificationsBulkAction.NoQualificationsSelected)]
        public List<Guid> SelectedQualificationIds { get; set; } = new();
        public QualificationsBulkActionInputViewModel BulkAction { get; set; } = new();
    }

}
