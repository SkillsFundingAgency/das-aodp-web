namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview.FundingApproval
{
    public class QfauFundingReviewOutcomeOffersSelectViewModel
    {
        public Guid ApplicationReviewId { get; set; }
        public List<Guid> SelectedOfferIds { get; set; } = new();

        public List<FundingOffer> FundingOffers { get; set; } = new();
        public class FundingOffer
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
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
