using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class GetApplicationFormStatusByApplicationIdQueryHandler : IRequestHandler<GetApplicationFormStatusByApplicationIdQuery, BaseMediatrResponse<GetApplicationFormStatusByApplicationIdQueryResponse>>
{
    private readonly IApiClient _apiClient;

    public GetApplicationFormStatusByApplicationIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetApplicationFormStatusByApplicationIdQueryResponse>> Handle(GetApplicationFormStatusByApplicationIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationFormStatusByApplicationIdQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetApplicationFormStatusByApplicationIdQueryResponse>(new GetApplicationFormStatusByApplicationIdApiRequest()
            {
                FormVersionId = request.FormVersionId,
                ApplicationId = request.ApplicationId
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