namespace SFA.DAS.AODP.Web.Models.FormBuilder.Page
{
    public class DeletePageViewModel
    {
        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }
        public Guid PageId { get; set; }
        public string Title { get; set; }
        public bool HasAssociatedRoutes { get; set; }
    }
}
