namespace SFA.DAS.AODP.Web.Models.Application
{
    public class ApplicationSectionViewModel
    {
        public Guid OrganisationId { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public string SectionTitle { get; set; }
        public string SectionDescription { get; set; }

        public bool IsCompleted { get; set; }
        public bool IsSubmitted { get; set; }


        public List<Page> Pages { get; set; }

        public class Page
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
            public bool Completed { get; set; }
        }
    }
}