using MediatR;
using SFA.DAS.AODP.Application;

public class SaveQanCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public Guid ApplicationReviewId { get; set; }
    public string? Qan { get; set; }
    public string SentByName { get; set; }
    public string SentByEmail { get; set; }
}
