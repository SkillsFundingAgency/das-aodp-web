using MediatR;
using SFA.DAS.AODP.Domain.Import;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Import;

public class GetJobRunsHandler : IRequestHandler<GetJobRunsQuery, BaseMediatrResponse<GetJobRunsQueryResponse>>
{
    private readonly IApiClient _apiClient;

    public GetJobRunsHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetJobRunsQueryResponse>> Handle(GetJobRunsQuery query, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetJobRunsQueryResponse>();
        response.Success = false;

        try
        {
            var result = await _apiClient.Get<GetJobRunsQueryResponse>(new GetJobRunsApiRequest() { JobName = query.JobName });            

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

