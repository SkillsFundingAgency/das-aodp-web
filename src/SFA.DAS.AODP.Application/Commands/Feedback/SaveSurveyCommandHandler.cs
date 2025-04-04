using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.Feedback;

public class SaveSurveyCommandHandler : IRequestHandler<SaveSurveyCommand, BaseMediatrResponse<EmptyResponse>>
{
    private readonly IApiClient _apiCLient;
    public SaveSurveyCommandHandler(IApiClient apiCLient)
    {
        _apiCLient = apiCLient;
    }


    public async Task<BaseMediatrResponse<EmptyResponse>> Handle(SaveSurveyCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<EmptyResponse>
        {
            Success = false
        };

        try
        {
            var result = await _apiCLient.PostWithResponseCode<EmptyResponse>(new SaveSurveyApiRequest()
            {
                Data = request
            });

           response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}