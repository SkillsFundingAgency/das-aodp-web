using MediatR;
using SFA.DAS.AODP.Application;


public class CreateQualificationDiscussionHistoryCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public Guid QualificationVersionId { get; set; }
    public Guid ActionTypeId { get; set; }
    public string? UserDisplayName { get; set; }
    public string? Notes { get; set; }
}
