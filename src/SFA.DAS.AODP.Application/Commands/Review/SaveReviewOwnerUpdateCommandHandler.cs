using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class SaveReviewOwnerUpdateCommandHandler : IRequestHandler<SaveReviewOwnerUpdateCommand, BaseMediatrResponse<EmptyResponse>>
{
    private readonly IApiClient _apiClient;


    public SaveReviewOwnerUpdateCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<EmptyResponse>> Handle(SaveReviewOwnerUpdateCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<EmptyResponse>()
        {
            Success = false
        };

        try
        {
            var apiRequest = new SaveReviewOwnerUpdateApiRequest(request.ApplicationReviewId)
            {
                Data = request
            };
            await _apiClient.Put(apiRequest);
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
