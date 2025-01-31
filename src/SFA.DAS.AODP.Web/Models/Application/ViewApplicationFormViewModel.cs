namespace SFA.DAS.AODP.Web.Models.Application
{
    public class ViewApplicationFormViewModel
    {
        public Guid OrganisationId { get; set; }
        public Guid FormVersionId { get; set; }
        public string ApplicationName { get; set; }
        public string FormName { get; set; }
        public string FormDescription { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsSubmitted { get; set; }

        public List<Section> Sections { get; set; }

        public class Section
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public int Order { get; set; }
            public int RemainingMandatoryQuestions { get; set; }
        }
    }
}