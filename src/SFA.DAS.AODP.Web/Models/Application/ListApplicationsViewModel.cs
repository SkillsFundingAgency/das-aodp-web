namespace SFA.DAS.AODP.Web.Models.Application
{
    public class ListApplicationsViewModel
    {
        public Guid OrganisationId { get; set; }

        public class Application
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime LastSubmittedDate { get; set; }

        }
    }
}