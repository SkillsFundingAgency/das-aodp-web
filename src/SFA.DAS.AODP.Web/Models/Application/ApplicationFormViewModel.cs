using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Models.Application;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Extensions;
using SFA.DAS.AODP.Web.Models.RelatedLinks;

namespace SFA.DAS.AODP.Web.Models.Application
{
    public class ApplicationFormViewModel : IHasRelatedLinks
    {
        public Guid OrganisationId { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public string FormTitle { get; set; }
        public string Reference { get; set; }
        public string? QualificationNumber { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public ApplicationStatus Status { get; set; }
        public bool CanWithdraw { get; set; }
        public bool NewMessage { get; set; }
        public bool VisibleToReviewers { get; set; }

        public bool IsCompleted { get; set; }
        public bool IsSubmitted { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public string Owner { get; set; }
        public List<Section> Sections { get; set; }

        public IReadOnlyList<RelatedLink> RelatedLinks { get; private set; } = Array.Empty<RelatedLink>();

        public void SetLinks(IUrlHelper url, UserType userType, RelatedLinksContext ctx)
            => RelatedLinks = RelatedLinksBuilder.Build(url, RelatedLinksPage.ApplyApplicationDetails, userType, ctx);

        public static ApplicationFormViewModel Map(GetApplicationFormByIdQueryResponse formsResponse,
            GetApplicationFormStatusByApplicationIdQueryResponse statusResponse,
            Guid formVersionId,
            Guid organisationId,
            Guid applicationId)
        {
            ApplicationFormViewModel model = new()
            {
                ApplicationId = applicationId,
                ApplicationName = statusResponse.ApplicationName,
                FormTitle = formsResponse.FormTitle,
                FormVersionId = formVersionId,
                IsCompleted = statusResponse.ReadyForSubmit,
                IsSubmitted = statusResponse.Submitted,
                SubmittedDate = statusResponse.SubmittedAt,
                OrganisationId = organisationId,
                Owner = statusResponse.Owner,
                Reference = statusResponse.Reference,
                QualificationNumber = statusResponse.QualificationNumber,
                Status = statusResponse.Status,
                NewMessage = statusResponse.NewMessage,
                UpdatedDate = statusResponse.UpdatedDate,
                VisibleToReviewers = statusResponse.ReviewExists,
                Sections = new(),
                CanWithdraw = statusResponse.Status.IsWithdrawable()
            };

            foreach (var section in formsResponse.Sections)
            {
                var modelSection = new Section()
                {
                    Order = section.Order,
                    Title = section.Title,
                    Id = section.Id,
                    PagesRemaining = statusResponse.Sections.First(s => s.SectionId == section.Id).PagesRemaining,
                    SkippedPages = statusResponse.Sections.First(s => s.SectionId == section.Id).SkippedPages,
                    TotalPages = statusResponse.Sections.First(s => s.SectionId == section.Id).TotalPages
                };

                if (modelSection.SkippedPages == modelSection.TotalPages) continue;


                model.Sections.Add(modelSection);
            }

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
}