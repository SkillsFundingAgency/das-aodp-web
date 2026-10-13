using static SFA.DAS.AODP.Web.Models.BulkActions.QualificationsBulkActionPageViewModel;

namespace SFA.DAS.AODP.Web.Models.BulkActions
{
    public class QualificationBulkActionPostModel
    {
        public List<Guid> SelectedQualificationIds { get; set; } = new();
        public QualificationsBulkActionInputViewModel BulkAction { get; set; } = new();
    }

}
