using MediatR;
using SFA.DAS.AODP.Application;


public class SaveQualificationsFundingOffersCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public Guid QualificationVersionId { get; set; }
    public List<Guid> SelectedOfferIds { get; set; } = new();
    public Guid QualificationId { get; set; }
    public string? QualificationReference { get; set; }
    public Guid ActionTypeId { get; set; }
    public string? UserDisplayName { get; set; }
}
