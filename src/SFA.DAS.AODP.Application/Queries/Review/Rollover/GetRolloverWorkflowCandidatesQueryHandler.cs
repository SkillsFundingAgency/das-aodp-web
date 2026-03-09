using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class GetRolloverWorkflowCandidatesQueryHandler : IRequestHandler<GetRolloverWorkflowCandidatesQuery, BaseMediatrResponse<GetRolloverWorkflowCandidatesQueryResponse>>
{
    private readonly IApiClient _apiClient;

    public GetRolloverWorkflowCandidatesQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetRolloverWorkflowCandidatesQueryResponse>> Handle(GetRolloverWorkflowCandidatesQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetRolloverWorkflowCandidatesQueryResponse>();
        response.Success = false;

        try
        {
            var result = await _apiClient.Get<GetRolloverWorkflowCandidatesQueryResponse>(new GetRolloverWorkflowCandidatesApiRequest(0, 0));
            response.Value = result;
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return await Task.FromResult(response);
    }
}