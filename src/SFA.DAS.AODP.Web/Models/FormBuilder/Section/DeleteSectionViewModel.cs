namespace SFA.DAS.AODP.Web.Models.FormBuilder.Section
{
    public class DeleteSectionViewModel
    {
        public string Title { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }
        public bool HasAssociatedRoutes { get; set; }
    }
}
