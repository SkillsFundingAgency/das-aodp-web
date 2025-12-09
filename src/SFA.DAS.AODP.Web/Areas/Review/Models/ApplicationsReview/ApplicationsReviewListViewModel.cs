using SFA.DAS.AODP.Models.Application;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview
{
    public class ApplicationsReviewListViewModel
    {
        public List<Application> Applications { get; set; } = new();
        public int? TotalItems { get; set; }


        public int Page { get; set; } = 1;
        public int ItemsPerPage { get; set; } = 10;

        public string? ApplicationSearch { get; set; }
        public string? AwardingOrganisationSearch { get; set; }
        public List<ApplicationStatus> Status { get; set; }
        public string UserType { get; set; }

        public class Application
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime LastUpdated { get; set; }
            public string? Owner { get; set; }
            public int Reference { get; set; }
            public string? Qan { get; set; }
            public ApplicationStatus Status { get; set; }
            public bool NewMessage { get; set; }
            public string? AwardingOrganisation { get; set; }
            public Guid ApplicationReviewId { get; set; }
        }

        public void MapApplications(GetApplicationsForReviewQueryResponse response)
        {
            TotalItems = response.TotalRecordsCount;
            foreach (var application in response.Applications)
            {
                Applications.Add(new()
                {
                    Id = application.Id,
                    Name = application.Name,
                    LastUpdated = application.LastUpdated,
                    Owner = application.Owner,
                    Reference = application.Reference,
                    Qan = application.Qan,
                    Status = application.Status,
                    AwardingOrganisation = application.AwardingOrganisation,
                    NewMessage = application.NewMessage,
                    ApplicationReviewId = application.ApplicationReviewId
                });
            }
        }
    }

}
