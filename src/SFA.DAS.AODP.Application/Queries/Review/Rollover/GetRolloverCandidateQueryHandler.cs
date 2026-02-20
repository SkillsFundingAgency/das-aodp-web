using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class GetRolloverCandidateQueryHandler : IRequestHandler<GetRolloverCandidateQuery, BaseMediatrResponse<GetRolloverCandidateQueryResponse>>
{
    private readonly IApiClient _apiClient;

    public GetRolloverCandidateQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetRolloverCandidateQueryResponse>> Handle(GetRolloverCandidateQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetRolloverCandidateQueryResponse>();
        response.Success = false;

        try
        {
            // TODO AWARD-1083: Replace with real API / DB call to get rollover candidate count.
            // Returning a dummy non-zero count to exercise the flow.
            response.Value = new GetRolloverCandidateQueryResponse
            {
                CandidateCount = 5
            };
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return await Task.FromResult(response);
    }
}