using MediatR;
using SFA.DAS.AODP.Application;

public class SaveSurveyCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public string Page { get; set; }

    public int SatisfactionScore { get; set; }

    public string Comments { get; set; }
}
