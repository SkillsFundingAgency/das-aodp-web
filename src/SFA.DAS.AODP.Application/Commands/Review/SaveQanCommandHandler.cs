using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
namespace SFA.DAS.AODP.Application.Commands.Application.Review;
public class SaveQanCommandHandler : IRequestHandler<SaveQanCommand, BaseMediatrResponse<SaveQanCommandResponse>>
{
    private readonly IApiClient _apiClient;


    public SaveQanCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<SaveQanCommandResponse>> Handle(SaveQanCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<SaveQanCommandResponse>()
        {
            Success = false,
            Value = new SaveQanCommandResponse()
        };

        try
        {
            var apiRequest = new SaveQanApiRequest(request.ApplicationReviewId)
            {
                Data = request
            };
            var result = await _apiClient.PutWithResponseCode<SaveQanCommandResponse>(apiRequest);

            response.Value.IsQanValid = result.Body.IsQanValid;
            response.Value.QanValidationMessage = result.Body.QanValidationMessage;

            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            response.Success = false;
        }

        return response;
    }
}
