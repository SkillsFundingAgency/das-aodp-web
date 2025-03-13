using SFA.DAS.AODP.Models.Application;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview.FundingApproval;

namespace SFA.DAS.AODP.Web.Models.Qualifications.Fundings
{
    public class QualificationFundingsOutcomeSummaryViewModel
    {
        public Guid ApplicationReviewId { get; set; }
        public string? Comments { get; set; }
        public ApplicationStatus? Status { get; set; }
        public bool Approved { get; set; }

        public List<OfferFunding> OfferFundingDetails { get; set; } = new();
        public List<FundingOffer> FundingOffers { get; set; } = new();



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

        public static QfauFundingReviewOutcomeSummaryViewModel Map(GetFeedbackForApplicationReviewByIdQueryResponse response, GetFundingOffersQueryResponse offers)
        {
            Enum.TryParse(response.Status, out ApplicationStatus status);
            QfauFundingReviewOutcomeSummaryViewModel model = new()
            {
                Approved = status == ApplicationStatus.Approved,
                Comments = response.Comments,
                Status = status
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

            model.MapOffers(offers);

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
