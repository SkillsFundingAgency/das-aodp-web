using Azure;
using System.ComponentModel;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview.FundingApproval
{
    public class QfauFundingReviewOutcomeOfferDetailsViewModel
    {
        public Guid ApplicationReviewId { get; set; }
        public List<OfferFundingDetails> Details { get; set; } = new();
        public List<FundingOffer> FundingOffers { get; set; } = new();

        public class OfferFundingDetails
        {
            public Guid FundingOfferId { get; set; }
            [DisplayName("Start date")]
            public DateOnly? StartDate { get; set; }
            [DisplayName("End date")]
            public DateOnly? EndDate { get; set; }
            public string? Comments { get; set; }
        }

        public class FundingOffer
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public static QfauFundingReviewOutcomeOfferDetailsViewModel Map(GetFeedbackForApplicationReviewByIdQueryResponse response, GetFundingOffersQueryResponse offers)
        {
            QfauFundingReviewOutcomeOfferDetailsViewModel model = new();

            foreach (var funding in response.FundedOffers ?? [])
            {
                model.Details.Add(new()
                {
                    Comments = funding.Comments,
                    EndDate = funding.EndDate,
                    StartDate = funding.StartDate,
                    FundingOfferId = funding.FundingOfferId,
                });
            }

            model.MapOffers(offers);

            return model;
        }

        public static SaveQfauFundingReviewOffersDetailsCommand Map(QfauFundingReviewOutcomeOfferDetailsViewModel model)
        {
            SaveQfauFundingReviewOffersDetailsCommand command = new()
            {
                ApplicationReviewId = model.ApplicationReviewId,

            };

            foreach (var funding in model.Details ?? [])
            {
                command.Details.Add(new()
                {
                    FundingOfferId = funding.FundingOfferId,
                    Comments = funding.Comments,
                    StartDate = funding.StartDate,
                    EndDate = funding.EndDate,
                });
            }

            return command;
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
