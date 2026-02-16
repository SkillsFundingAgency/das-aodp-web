using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class SaveReviewerCommandHandler : IRequestHandler<SaveReviewerCommand, BaseMediatrResponse<SaveReviewerCommandResponse>>
{
    private readonly IApiClient _apiClient;

    public SaveReviewerCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<SaveReviewerCommandResponse>> Handle(SaveReviewerCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<SaveReviewerCommandResponse>()
        {
            Success = false
        };

        try
        {
            var apiRequest = new SaveReviewerApiRequest(request.ApplicationId)
            {
                Data = request
            };
            var result = await _apiClient.Put<SaveReviewerCommandResponse>(apiRequest);
            response.Success = true;
            response.Value = result;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            response.Success = false;
        }

        return response;
    }
}
