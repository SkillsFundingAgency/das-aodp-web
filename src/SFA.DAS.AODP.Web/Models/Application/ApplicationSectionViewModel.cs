using SFA.DAS.AODP.Models.Forms;

namespace SFA.DAS.AODP.Web.Models.Application
{
    public class ApplicationSectionViewModel
    {
        public Guid OrganisationId { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }
        public Guid ApplicationId { get; set; }
        public string SectionTitle { get; set; }


        public List<Page> Pages { get; set; } = new();

        public static ApplicationSectionViewModel Map(GetApplicationSectionByIdQueryResponse section, GetApplicationSectionStatusByApplicationIdQueryResponse status, Guid organisationId, Guid formVersionId, Guid sectionId, Guid applicationId)
        {
            ApplicationSectionViewModel model = new()
            {
                SectionTitle = section.SectionTitle,
                OrganisationId = organisationId,
                SectionId = sectionId,
                FormVersionId = formVersionId,
                ApplicationId = applicationId
            };

            foreach (var page in section.Pages)
            {
                var statusPage = status.Pages.FirstOrDefault(p => p.PageId == page.Id);
                if (statusPage?.Status == ApplicationPageStatus.Skipped.ToString()) continue;
                Page modelPage = new()
                {
                    Id = page.Id,
                    Title = page.Title,
                    Order = page.Order,
                };

                modelPage.Completed = statusPage?.Status == ApplicationPageStatus.Completed.ToString();
                model.Pages.Add(modelPage);
            }

            return model;

        }

        public class Page
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
            public bool Completed { get; set; }
        }
    }
}