using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class GetRolloverCandidatesQueryHandler : IRequestHandler<GetRolloverCandidatesQuery, BaseMediatrResponse<GetRolloverCandidatesQueryResponse>>
{
    private readonly IApiClient _apiClient;

    public GetRolloverCandidatesQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetRolloverCandidatesQueryResponse>> Handle(GetRolloverCandidatesQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetRolloverCandidatesQueryResponse>();
        response.Success = false;

        try
        {
            var result = await _apiClient.Get<GetRolloverCandidatesQueryResponse>(new GetRolloverCandidatesApiRequest());
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