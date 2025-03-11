using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class GetApplicationsForReviewQueryHandler : IRequestHandler<GetApplicationsForReviewQuery, BaseMediatrResponse<GetApplicationsForReviewQueryResponse>>
{
    private readonly IApiClient _apiCLient;
    public GetApplicationsForReviewQueryHandler(IApiClient apiCLient)
    {
        _apiCLient = apiCLient;
    }

    public async Task<BaseMediatrResponse<GetApplicationsForReviewQueryResponse>> Handle(GetApplicationsForReviewQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationsForReviewQueryResponse>
        {
            Success = false
        };

        try
        {
            var result =  await _apiCLient.PostWithResponseCode<GetApplicationsForReviewQueryResponse>(new GetApplicationsForReviewApiRequest()
            {
                Data = request
            });

            response.Value = result;

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