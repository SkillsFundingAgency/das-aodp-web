namespace SFA.DAS.AODP.Web.Models.Application
{
    public class ListApplicationsViewModel
    {
        public Guid OrganisationId { get; set; }
        public List<Application> Applications { get; set; }

        public class Application
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime LastSubmittedDate { get; set; }
            public bool Submitted { get; set; }
            public string Owner { get; set; }
            public string Reference { get; set; }

        }
    }
}