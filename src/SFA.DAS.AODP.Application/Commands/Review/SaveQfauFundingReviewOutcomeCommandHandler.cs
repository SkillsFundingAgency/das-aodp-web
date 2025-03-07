using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class SaveQfauFundingReviewOutcomeCommandHandler : IRequestHandler<SaveQfauFundingReviewOutcomeCommand, BaseMediatrResponse<EmptyResponse>>
{
    private readonly IApiClient _apiClient;


    public SaveQfauFundingReviewOutcomeCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<EmptyResponse>> Handle(SaveQfauFundingReviewOutcomeCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<EmptyResponse>()
        {
            Success = false
        };

        try
        {
            var apiRequest = new SaveQfauFundingReviewOutcomeApiRequest()
            {
                ApplicationReviewId = request.ApplicationReviewId,
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