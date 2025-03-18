using SFA.DAS.AODP.Models.Users;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview
{
    public class ApplicationReviewViewModel
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

        public List<Funding> FundedOffers { get; set; } = new();
        public List<Feedback> Feedbacks { get; set; } = new();

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
