using MediatR;
using SFA.DAS.AODP.Application;


public class SaveQfauFundingReviewOffersCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public Guid ApplicationReviewId { get; set; }
    public List<Guid> SelectedOfferIds { get; set; } = new();
}
