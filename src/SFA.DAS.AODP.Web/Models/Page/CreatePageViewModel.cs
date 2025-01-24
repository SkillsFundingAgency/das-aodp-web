namespace SFA.DAS.AODP.Web.Models.Page
{
    public class CreatePageViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }

    }
}
