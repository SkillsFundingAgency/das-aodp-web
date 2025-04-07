using MediatR;
using SFA.DAS.AODP.Application;


public class CreateQualificationDiscussionHistoryNoteForFundingOffersCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public Guid QualificationVersionId { get; set; }
    public Guid QualificationId { get; set; }
    public string? QualificationReference { get; set; }
    public Guid ActionTypeId { get; set; }
    public string? UserDisplayName { get; set; }
}
