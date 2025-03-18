using MediatR;
using SFA.DAS.AODP.Application;

public class SaveOfqualReviewOutcomeCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public Guid ApplicationReviewId { get; set; }
    public string? Comments { get; set; }
    public string SentByName { get; set; }
    public string SentByEmail { get; set; }
}
