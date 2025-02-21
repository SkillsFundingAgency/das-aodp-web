using SFA.DAS.AODP.Application.Queries.Application.Form;

namespace SFA.DAS.AODP.Web.Models.Application;

public class ApplicationFormPreviewViewModel
{
    public Guid OrganisationId { get; set; }
    public Guid FormVersionId { get; set; }
    public Guid ApplicationId { get; set; }

    public string FormTitle { get; set; }

    public static ApplicationFormPreviewViewModel Map(GetFormPreviewByIdQueryResponse formPreviewResponse,
        Guid formVersionId,
        Guid organisationId,
        Guid applicationId)
    {
        ApplicationFormPreviewViewModel model = new()
        {
            ApplicationId = applicationId,
            OrganisationId = organisationId,
            FormTitle = "DUMMY TITLE",
            FormVersionId = formVersionId
        };

        return model;
    }

    public class Section
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public int PagesRemaining { get; set; }
        public int SkippedPages { get; set; }
        public int TotalPages { get; set; }
    }
}
