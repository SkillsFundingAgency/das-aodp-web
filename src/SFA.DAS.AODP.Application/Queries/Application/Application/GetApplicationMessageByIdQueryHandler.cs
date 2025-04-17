using MediatR;
using SFA.DAS.AODP.Domain.Application.Application;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Application.Application;

public class GetApplicationMessageByIdQueryHandler : IRequestHandler<GetApplicationMessageByIdQuery, BaseMediatrResponse<GetApplicationMessageByIdQueryResponse>>
{
    private readonly IApiClient _apiClient;
    public GetApplicationMessageByIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetApplicationMessageByIdQueryResponse>> Handle(GetApplicationMessageByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationMessageByIdQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetApplicationMessageByIdQueryResponse>(new GetApplicationMessageByIdApiRequest()
            {
                MessageId = request.MessageId,

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