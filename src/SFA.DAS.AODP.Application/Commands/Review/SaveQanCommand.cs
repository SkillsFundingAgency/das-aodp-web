using MediatR;
using SFA.DAS.AODP.Application;
namespace SFA.DAS.AODP.Application.Commands.Application.Review;
public class SaveQanCommand : IRequest<BaseMediatrResponse<SaveQanCommandResponse>>
{
    public Guid ApplicationReviewId { get; set; }
    public string? Qan { get; set; }
    public string SentByName { get; set; }
    public string SentByEmail { get; set; }
}
