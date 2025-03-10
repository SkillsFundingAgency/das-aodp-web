using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Application.Application;

public class GetApplicationsByOrganisationIdQueryHandler : IRequestHandler<GetApplicationsByOrganisationIdQuery, BaseMediatrResponse<GetApplicationsByOrganisationIdQueryResponse>>
{
    private readonly IApiClient _apiClient;

    public GetApplicationsByOrganisationIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetApplicationsByOrganisationIdQueryResponse>> Handle(GetApplicationsByOrganisationIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationsByOrganisationIdQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetApplicationsByOrganisationIdQueryResponse>(new GetApplicationsByOrganisationIdApiRequest());
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
