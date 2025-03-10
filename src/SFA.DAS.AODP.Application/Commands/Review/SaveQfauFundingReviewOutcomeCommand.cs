using MediatR;
using SFA.DAS.AODP.Application;

public class SaveQfauFundingReviewOutcomeCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public Guid ApplicationReviewId { get; set; }
    public string? Comments { get; set; }
    public bool Approved { get; set; }
}
