namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class GetTypesForRolloverQueryBuilderQueryHandler(IApiClient apiClient)
    : IRequestHandler<GetTypesForRolloverQueryBuilderQuery, BaseMediatrResponse<GetTypesForRolloverQueryBuilderQueryResponse>>
{
    private readonly IApiClient _apiClient = apiClient;

    public async Task<BaseMediatrResponse<GetTypesForRolloverQueryBuilderQueryResponse>> Handle(
        GetTypesForRolloverQueryBuilderQuery request,
        CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetTypesForRolloverQueryBuilderQueryResponse>();

        try
        {
            var result = await _apiClient.PostWithResponseCode<GetTypesForRolloverQueryBuilderQueryResponse>(new GetTypesForRolloverQueryBuilderApiRequest(request.Filters));
            
            if (result is null)
            {
                response.Success = true;
                response.Value.Types = [];
                
                return response;
            }

            response.Value.Types = result.Types.Select(o => QualificationType.FromId(o.Id));
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