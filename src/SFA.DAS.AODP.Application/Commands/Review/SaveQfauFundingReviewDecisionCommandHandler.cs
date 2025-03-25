using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class SaveQfauFundingReviewDecisionCommandHandler : IRequestHandler<SaveQfauFundingReviewDecisionCommand, BaseMediatrResponse<EmptyResponse>>
{
    private readonly IApiClient _apiClient;


    public SaveQfauFundingReviewDecisionCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<EmptyResponse>> Handle(SaveQfauFundingReviewDecisionCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<EmptyResponse>()
        {
            Success = false
        };

        try
        {
            var apiRequest = new SaveQfauFundingReviewDecisionApiRequest(request.ApplicationReviewId)
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