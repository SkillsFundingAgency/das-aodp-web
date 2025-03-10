using MediatR;
using SFA.DAS.AODP.Domain.Application.Application;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Application.Application;

public class GetApplicationMessagesByIdQueryHandler : IRequestHandler<GetApplicationMessagesByIdQuery, BaseMediatrResponse<GetApplicationMessagesByIdQueryResponse>>
{
    private readonly IApiClient _apiClient;
    public GetApplicationMessagesByIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetApplicationMessagesByIdQueryResponse>> Handle(GetApplicationMessagesByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationMessagesByIdQueryResponse>();
        response.Success = false;
        try
        {
            if (request.UserType == null) { }

            var result = await _apiClient.Get<GetApplicationMessagesByIdQueryResponse>(new GetApplicationMessagesByIdApiRequest()
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