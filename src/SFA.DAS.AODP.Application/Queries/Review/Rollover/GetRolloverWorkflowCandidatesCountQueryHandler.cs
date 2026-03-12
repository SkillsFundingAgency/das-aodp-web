using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class GetRolloverWorkflowCandidatesCountQueryHandler : IRequestHandler<GetRolloverWorkflowCandidatesCountQuery, BaseMediatrResponse<GetRolloverWorkflowCandidatesCountQueryResponse>>
{
    private readonly IApiClient _apiClient;

    public GetRolloverWorkflowCandidatesCountQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetRolloverWorkflowCandidatesCountQueryResponse>> Handle(GetRolloverWorkflowCandidatesCountQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetRolloverWorkflowCandidatesCountQueryResponse>();
        response.Success = true;

        try
        {
            var result = await _apiClient.Get<BaseMediatrResponse<GetRolloverWorkflowCandidatesCountQueryResponse>>(new GetRolloverWorkflowCandidatesCountApiRequest());
            response.Value.TotalRecords = result.Value.TotalRecords;
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = ex.Message;
        }

        return await Task.FromResult(response);
    }
}