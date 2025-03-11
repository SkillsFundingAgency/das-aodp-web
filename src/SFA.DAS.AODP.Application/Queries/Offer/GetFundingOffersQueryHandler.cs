using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class GetFundingOffersQueryHandler : IRequestHandler<GetFundingOffersQuery, BaseMediatrResponse<GetFundingOffersQueryResponse>>
{
    private readonly IApiClient _apiClient;

    public GetFundingOffersQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetFundingOffersQueryResponse>> Handle(GetFundingOffersQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetFundingOffersQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetFundingOffersQueryResponse>(new GetFundingOffersApiRequest());
            response.Value = result;
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}