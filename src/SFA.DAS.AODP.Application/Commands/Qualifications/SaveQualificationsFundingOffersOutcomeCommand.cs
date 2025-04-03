using MediatR;
using SFA.DAS.AODP.Application;

public class SaveQualificationsFundingOffersOutcomeCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public Guid QualificationVersionId { get; set; }
    public string? Comments { get; set; }
    public bool? Approved { get; set; }
    public Guid QualificationId { get; set; }
    public string? QualificationReference { get; set; }
    public Guid ActionTypeId { get; set; }
    public string? UserDisplayName { get; set; }
}
