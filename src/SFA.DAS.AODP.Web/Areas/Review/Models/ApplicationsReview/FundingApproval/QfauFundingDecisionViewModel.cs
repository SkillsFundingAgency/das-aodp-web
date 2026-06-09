using SFA.DAS.AODP.Models.Application;
using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview.FundingApproval;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview
{
    public class QfauFundingDecisionViewModel
    {
        public Guid ApplicationReviewId { get; set; }
        public string? Comments { get; set; }
        public ApplicationStatus? Status { get; set; }

        public bool CanSubmit { get; set; }
        public List<string> Messages { get; set; } = new();

        public List<OfferFunding> OfferFundingDetails { get; set; } = new();
        public List<FundingOffer> FundingOffers { get; set; } = new();
        public Qualification? RelatedQualification { get; set; }


        public class OfferFunding
        {
            public Guid FundingOfferId { get; set; }
            public DateOnly? StartDate { get; set; }
            public DateOnly? EndDate { get; set; }
            public string? Comments { get; set; }
        }

        public class FundingOffer
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public class Qualification
        {
            public string? Qan { get; set; }
            public string? Status { get; set; }
            public string? Name { get; set; }
        }

        public static QfauFundingDecisionViewModel Map(Guid applicationReviewid, GetQfauFeedbackForApplicationReviewConfirmationQueryResponse response, GetFundingOffersQueryResponse offers)
        {
            Enum.TryParse(response.Status, out ApplicationStatus status);
            QfauFundingDecisionViewModel model = new()
            {
                ApplicationReviewId = applicationReviewid,
                Comments = response.Comments,
                Status = status,
            };

            foreach (var funding in response.FundedOffers ?? [])
            {
                model.OfferFundingDetails.Add(new()
                {
                    Comments = funding.Comments,
                    EndDate = funding.EndDate,
                    StartDate = funding.StartDate,
                    FundingOfferId = funding.FundingOfferId,
                });
            }

            if (response.RelatedQualification != null)
            {
                model.RelatedQualification = new()
                {
                    Qan = response.RelatedQualification.Qan,
                    Status = response.RelatedQualification.Status,
                    Name = response.RelatedQualification.Name,
                };
            }

            model.MapOffers(offers);

            var messages = new List<string>();

            var qualificationStatus = model.RelatedQualification?.Status;

            if (model.Status == ApplicationStatus.NotApproved && model.OfferFundingDetails.Count > 0)
            {
                messages.Add(FundingDecisionMessages.NotApprovedWithOffers);
            }
            else if (model.Status != ApplicationStatus.NotApproved && model.Status != ApplicationStatus.Approved)
            {
                messages.Add(FundingDecisionMessages.InvalidStatus);
            }
            else if (model.Status == ApplicationStatus.Approved && model.RelatedQualification == null)
            {
                messages.Add(FundingDecisionMessages.MissingQualification);
            }
            else if (qualificationStatus == ProcessStatusLookup.Rejected.Name || qualificationStatus == ProcessStatusLookup.NoActionRequired.Name)
            {
                messages.Add(FundingDecisionMessages.InvalidQualificationStatus(qualificationStatus));
            }

            model.Messages = messages;
            model.CanSubmit = ! (messages.Count > 0);

            return model;
        }

        public void MapOffers(GetFundingOffersQueryResponse response)
        {
            FundingOffers = new();

            foreach (var offer in response.Offers)
            {
                FundingOffers.Add(new()
                {
                    Id = offer.Id,
                    Name = offer.Name,
                });
            }
        }
    }
}
