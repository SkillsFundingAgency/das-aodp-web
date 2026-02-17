using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.Review;
using SFA.DAS.AODP.Models.Application;
using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Models.RelatedLinks;
using SFA.DAS.AODP.Web.Constants;


namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview
{
    public class ApplicationReviewViewModel : IHasRelatedLinks
    {
        public UserType UserType { get; set; }
        public Guid Id { get; set; }
        public Guid ApplicationReviewId { get; set; }
       
        public string Name { get; set; }
        public DateTime LastUpdated { get; set; }
        public int Reference { get; set; }
        public string? Qan { get; set; }
        public string? AwardingOrganisation { get; set; }

        public bool SharedWithSkillsEngland { get; set; }
        public bool SharedWithOfqual { get; set; }

        public string FormTitle { get; set; }

        public ApplicationStatus ApplicationStatus { get; set; }
        public List<Funding> FundedOffers { get; set; } = new();
        public List<Feedback> Feedbacks { get; set; } = new();

        public IReadOnlyList<RelatedLink> RelatedLinks { get; private set; } = [];

        public void SetLinks(IUrlHelper url, UserType userType, RelatedLinksContext ctx)
            => RelatedLinks = RelatedLinksBuilder.Build(url, RelatedLinksPage.ReviewApplicationDetails, userType, ctx);

        public string? Reviewer1 { get; set; }
        public string? Reviewer2 { get; set; }
        public List<SelectListItem> ReviewerOptions { get; set; } = new();

        public class Feedback
        {
            public string? Owner { get; set; }
            public string Status { get; set; }
            public bool NewMessage { get; set; }
            public string UserType { get; set; }
            public string? Comments { get; set; }
            public bool LatestCommunicatedToAwardingOrganisation { get; set; }
        }

        public class Funding
        {
            public Guid Id { get; set; }
            public Guid FundingOfferId { get; set; }
            public string FundedOfferName { get; set; }
            public DateOnly? StartDate { get; set; }
            public DateOnly? EndDate { get; set; }
            public string? Comments { get; set; }
        }

        public static ApplicationReviewViewModel Map(GetApplicationForReviewByIdQueryResponse response, UserType userType)

        {
            Enum.TryParse(response.ApplicationStatus, out ApplicationStatus applicationStatus);
            ApplicationReviewViewModel model = new()
            {
                Id = response.Id,
                ApplicationReviewId = response.ApplicationReviewId,
                AwardingOrganisation = response.AwardingOrganisation,
                LastUpdated = response.LastUpdated,
                Name = response.Name,
                Qan = response.Qan,
                Reference = response.Reference,
                SharedWithOfqual = response.SharedWithOfqual,
                SharedWithSkillsEngland = response.SharedWithSkillsEngland,
                UserType = userType,
                FormTitle = response.FormTitle,
                ApplicationStatus = applicationStatus,

                Reviewer1 = string.IsNullOrWhiteSpace(response.Reviewer1)
                    ? ReviewerDropdown.Assignment.UnassignedValue
                    : response.Reviewer1,
                Reviewer2 = string.IsNullOrWhiteSpace(response.Reviewer2)
                    ? ReviewerDropdown.Assignment.UnassignedValue
                    : response.Reviewer2,
                ReviewerOptions =
                    response.AvailableReviewers
                        .OrderBy(r => r.LastName).ThenBy(r => r.FirstName)
                        .Select(r => new SelectListItem
                        {
                            Value = $"{r.FirstName} {r.LastName}",
                            Text = $"{r.FirstName} {r.LastName}"
                        })
                        .Concat(new[]
                        {
                            new SelectListItem
                            {
                                Value = ReviewerDropdown.Assignment.UnassignedValue,
                                Text  = ReviewerDropdown.Assignment.UnassignedText
                            }
                        })
                        .ToList(),

            };

            foreach (var feedback in response.Feedbacks)
            {
                model.Feedbacks.Add(new()
                {
                    Comments = feedback.Comments,
                    NewMessage = feedback.NewMessage,
                    Owner = feedback.Owner,
                    Status = feedback.Status,
                    UserType = feedback.UserType,
                    LatestCommunicatedToAwardingOrganisation = feedback.LatestCommunicatedToAwardingOrganisation
                });
            }

            foreach (var funding in response.FundedOffers)
            {
                model.FundedOffers.Add(new()
                {
                    Comments = funding.Comments,
                    EndDate = funding.EndDate,
                    FundedOfferName = funding.FundedOfferName,
                    FundingOfferId = funding.Id,
                    StartDate = funding.StartDate,
                    Id = funding.Id,
                });
            }

            return model;
        }
    }

}
