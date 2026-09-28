using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class RemovePreviousWorkflowCandidatesQueryHandler(IApiClient apiClient)
    : IRequestHandler<RemovePreviousWorkflowCandidatesQuery, BaseMediatrResponse<EmptyResponse>>
{
    public async Task<BaseMediatrResponse<EmptyResponse>> Handle(RemovePreviousWorkflowCandidatesQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<EmptyResponse>
        {
            Success = true
        };

        try
        {
            await apiClient.PostWithResponseCode<EmptyResponse>(new RemovePreviousWorkflowCandidatesApiRequest());
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
