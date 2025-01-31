namespace SFA.DAS.AODP.Web.Models.Application
{
    public class ViewApplicationSectionViewModel
    {
        public Guid OrganisationId { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }
        public string ApplicationName { get; set; }
        public string FormName { get; set; }
        public string FormDescription { get; set; }

        public bool IsCompleted { get; set; }
        public bool IsSubmitted { get; set; }


        public List<Page> Pages { get; set; }

        public class Page
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public int Order { get; set; }
            public bool Completed { get; set; }
        }
    }
}