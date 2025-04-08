using MediatR;
using SFA.DAS.AODP.Application;

public class SaveQualificationsFundingOffersDetailsCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public Guid QualificationVersionId { get; set; }
    public List<OfferFundingDetails> Details { get; set; } = new();
    public Guid QualificationId { get; set; }
    public string? QualificationReference { get; set; }
    public Guid ActionTypeId { get; set; }
    public string? UserDisplayName { get; set; }

    public class OfferFundingDetails
    {
        public Guid FundingOfferId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Comments { get; set; }
    }
}
