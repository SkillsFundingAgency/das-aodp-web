using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;

public class GetAllFormVersionsQueryHandler : IRequestHandler<GetAllFormVersionsQuery, BaseMediatrResponse<GetAllFormVersionsQueryResponse>>
{
    private readonly IApiClient _apiClient;


    public GetAllFormVersionsQueryHandler(IApiClient aodpApiClient)
    {
        _apiClient = aodpApiClient;

    }

    public async Task<BaseMediatrResponse<GetAllFormVersionsQueryResponse>> Handle(GetAllFormVersionsQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetAllFormVersionsQueryResponse>
        {
            Success = false
        };

        try
        {
            var result = await _apiClient.Get<GetAllFormVersionsQueryResponse>(new GetAllFormVersionsApiRequest());
            response.Value = result;
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