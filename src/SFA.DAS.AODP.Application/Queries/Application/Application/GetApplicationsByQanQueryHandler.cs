using MediatR;
using SFA.DAS.AODP.Domain.Application.Application;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Application.Application;

public class GetApplicationsByQanQueryHandler : IRequestHandler<GetApplicationsByQanQuery, BaseMediatrResponse<GetApplicationsByQanQueryResponse>>
{
    private readonly IApiClient _apiClient;

    public GetApplicationsByQanQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    public async Task<BaseMediatrResponse<GetApplicationsByQanQueryResponse>> Handle(GetApplicationsByQanQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationsByQanQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetApplicationsByQanQueryResponse>(new GetApplicationsByQanApiRequest()
            {
                Qan = request.Qan
            });
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
