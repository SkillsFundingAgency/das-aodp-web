using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;

public class GetFormVersionByIdQueryHandler : IRequestHandler<GetFormVersionByIdQuery, BaseMediatrResponse<GetFormVersionByIdQueryResponse>>
{
    private readonly IApiClient _apiClient;


    public GetFormVersionByIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<BaseMediatrResponse<GetFormVersionByIdQueryResponse>> Handle(GetFormVersionByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetFormVersionByIdQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetFormVersionByIdQueryResponse>(new GetFormVersionByIdApiRequest(request.FormVersionId));
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
