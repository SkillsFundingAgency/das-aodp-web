using MediatR;
using SFA.DAS.AODP.Domain.Application.Application;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Application.Application;

public class GetApplicationMessagesByApplicationIdQueryHandler : IRequestHandler<GetApplicationMessagesByApplicationIdQuery, BaseMediatrResponse<GetApplicationMessagesByApplicationIdQueryResponse>>
{
    private readonly IApiClient _apiClient;
    public GetApplicationMessagesByApplicationIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetApplicationMessagesByApplicationIdQueryResponse>> Handle(GetApplicationMessagesByApplicationIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationMessagesByApplicationIdQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetApplicationMessagesByApplicationIdQueryResponse>(new GetApplicationMessagesByApplicationIdApiRequest()
            {
                ApplicationId = request.ApplicationId,
                UserType = request.UserType
                
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
