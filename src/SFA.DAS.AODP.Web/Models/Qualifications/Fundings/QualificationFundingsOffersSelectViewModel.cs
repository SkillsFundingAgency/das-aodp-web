namespace SFA.DAS.AODP.Web.Models.Qualifications.Fundings
{
    public class QualificationFundingsOffersSelectViewModel
    {
        public Guid QualificationVersionId { get; set; }
        public List<Guid> SelectedOfferIds { get; set; } = new();
        public Guid QualificationId { get; set; }
        public string? QualificationReference { get; set; }
        public List<FundingOffer> FundingOffers { get; set; } = new();
        public string Mode { get; set; }
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
