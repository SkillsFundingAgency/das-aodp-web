namespace SFA.DAS.AODP.Web.Models.Application
{
    public class ApplicationFormViewModel
    {
        public Guid OrganisationId { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public string FormTitle { get; set; }
        public string Reference { get; set; }

        public bool IsCompleted { get; set; }
        public bool IsSubmitted { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public string Owner { get; set; }
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