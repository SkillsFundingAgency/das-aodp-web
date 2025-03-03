using SFA.DAS.AODP.Models.Application;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview
{
    public class ApplicationsReviewListViewModel
    {
        public List<Application> Applications { get; set; } = new();
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }

        public string? ApplicationSearch { get; set; }
        public string? AwardingOrganisationSearch { get; set; }
        public List<ApplicationStatus>? StatusSearch { get; set; }

        public class Application
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime LastUpdated { get; set; }
            public string Owner { get; set; }
            public string Reference { get; set; }
            public string? Qan { get; set; }
            public string Status { get; set; }
        }
    }
}
