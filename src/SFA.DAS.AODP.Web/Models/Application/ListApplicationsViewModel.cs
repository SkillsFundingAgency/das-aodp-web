using SFA.DAS.AODP.Models.Application;

namespace SFA.DAS.AODP.Web.Models.Application
{
    public class ListApplicationsViewModel
    {
        public Guid OrganisationId { get; set; }
        public List<Application> Applications { get; set; }

        public static ListApplicationsViewModel Map(GetApplicationsByOrganisationIdQueryResponse value, Guid organisationId)
        {
            ListApplicationsViewModel model = new()
            {
                OrganisationId = organisationId,
                Applications = new()
            };

            foreach (var application in value.Applications)
            {
                model.Applications.Add(new()
                {
                    Id = application.Id,
                    CreatedDate = application.CreatedDate,
                    Name = application.Name,
                    Owner = application.Owner,
                    Reference = application.Reference,
                    Submitted = application.Submitted,
                    SubmittedDate = application.SubmittedDate,
                    FormVersionId = application.FormVersionId,
                    UpdatedDate = application.UpdatedDate,
                    Status = application.Status,
                    NewMessage = application.NewMessage,
                });
            }

            return model;
        }

        public class Application
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime? CreatedDate { get; set; }
            public DateTime? SubmittedDate { get; set; }
            public bool Submitted { get; set; }
            public string Owner { get; set; }
            public string Reference { get; set; }
            public Guid FormVersionId { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public ApplicationStatus Status { get; set; }
            public bool NewMessage { get; set; }

        }
    }
}