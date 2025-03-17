using MediatR;
using SFA.DAS.AODP.Application;

public class SaveQualificationsFundingOffersOutcomeCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public Guid QualificationVersionId { get; set; }
    public string? Comments { get; set; }
    public bool Approved { get; set; }
}
