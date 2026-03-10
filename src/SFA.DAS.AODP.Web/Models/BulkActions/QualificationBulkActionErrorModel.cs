namespace SFA.DAS.AODP.Web.Models.BulkActions
{
    public class QualificationBulkActionErrorModel
    {
        public List<QualificationBulkActionErrorItemViewModel> Failed { get; set; } = new();
        public string BackLinkText { get; set; } = string.Empty;
        public string BackLinkUrl { get; set; } = string.Empty;
    }

    public class QualificationBulkActionErrorItemViewModel
    {
        public Guid QualificationId { get; set; }
        public string Qan { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string FailureReason { get; set; } = string.Empty;
    }
}
