using MediatR;
using SFA.DAS.AODP.Application;


public class SaveQualificationsFundingOffersCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public Guid QualificationVersionId { get; set; }
    public List<Guid> SelectedOfferIds { get; set; } = new();
}
