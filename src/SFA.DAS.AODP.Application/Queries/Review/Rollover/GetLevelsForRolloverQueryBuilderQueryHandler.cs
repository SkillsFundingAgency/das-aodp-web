namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class GetLevelsForRolloverQueryBuilderQueryHandler(IApiClient apiClient)
    : IRequestHandler<GetLevelsForRolloverQueryBuilderQuery, BaseMediatrResponse<GetLevelsForRolloverQueryBuilderQueryResponse>>
{
    private readonly IApiClient _apiClient = apiClient;

    public async Task<BaseMediatrResponse<GetLevelsForRolloverQueryBuilderQueryResponse>> Handle(
        GetLevelsForRolloverQueryBuilderQuery request,
        CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetLevelsForRolloverQueryBuilderQueryResponse>();

        try
        {
            var result = await _apiClient.Get<GetLevelsForRolloverQueryBuilderQueryResponse>(new GetLevelsForRolloverQueryBuilderApiRequest());
            response.Value.Levels = result.Levels.Select(o => QualificationLevel.FromId(o.Id));
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