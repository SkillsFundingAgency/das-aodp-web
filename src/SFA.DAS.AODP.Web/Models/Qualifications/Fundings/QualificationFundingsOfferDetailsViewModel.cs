using System.ComponentModel;

namespace SFA.DAS.AODP.Web.Models.Qualifications.Fundings
{
    public class QualificationFundingsOfferDetailsViewModel
    {
        public Guid QualificationVersionId { get; set; }
        public List<OfferFundingDetails> Details { get; set; } = new();
        public List<FundingOffer> FundingOffers { get; set; } = new();
        public Guid QualificationId { get; set; }
        public string? QualificationReference { get; set; }

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

        public static QualificationFundingsOfferDetailsViewModel Map(GetFeedbackForQualificationFundingByIdQueryResponse response, GetFundingOffersQueryResponse offers)
        {
            QualificationFundingsOfferDetailsViewModel model = new();
            model.QualificationVersionId = response.QualificationVersionId;
            foreach (var funding in response.QualificationFundedOffers ?? [])
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

        public static SaveQualificationsFundingOffersDetailsCommand Map(QualificationFundingsOfferDetailsViewModel model, string userDisplayName, Guid actiontypeId)
        {
            SaveQualificationsFundingOffersDetailsCommand command = new()
            {
                QualificationVersionId = model.QualificationVersionId,
                QualificationReference = model.QualificationReference,
                QualificationId = model.QualificationId,
                UserDisplayName = userDisplayName,
                ActionTypeId = actiontypeId,
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
