using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Application.Form;
public class GetApplicationFormsQueryHandler : IRequestHandler<GetApplicationFormsQuery, BaseMediatrResponse<GetApplicationFormsQueryResponse>>
{
    private readonly IApiClient _apiClient;

    public GetApplicationFormsQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetApplicationFormsQueryResponse>> Handle(GetApplicationFormsQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationFormsQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetApplicationFormsQueryResponse>(new GetApplicationFormsApiRequest());
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