using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;

namespace SFA.DAS.AODP.Web.Models.FormBuilder.Page
{
    public class PreviewPageViewModel
    {
        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }
        public Guid PageId { get; set; }
        public GetPagePreviewByIdQueryResponse Value { get; set; }
    }
}
