using MediatR;
using SFA.DAS.AODP.Domain.Application.Form;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Application.Form;

public class GetFormPreviewByIdQueryHandler : IRequestHandler<GetFormPreviewByIdQuery, BaseMediatrResponse<GetFormPreviewByIdQueryResponse>>
{
    private readonly IApiClient _apiClient;


    public GetFormPreviewByIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetFormPreviewByIdQueryResponse>> Handle(GetFormPreviewByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetFormPreviewByIdQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetFormPreviewByIdQueryResponse>(new GetFormPreviewByIdApiRequest(request.ApplicationId));
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
