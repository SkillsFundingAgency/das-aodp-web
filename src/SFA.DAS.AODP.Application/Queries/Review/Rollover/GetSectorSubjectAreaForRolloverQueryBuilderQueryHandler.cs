namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class GetSectorSubjectAreaForRolloverQueryBuilderQueryHandler(IApiClient apiClient)
    : IRequestHandler<GetSectorSubjectAreaForRolloverQueryBuilderQuery, BaseMediatrResponse<GetSectorSubjectAreaForRolloverQueryBuilderQueryResponse>>
{
    private readonly IApiClient _apiClient = apiClient;

    public async Task<BaseMediatrResponse<GetSectorSubjectAreaForRolloverQueryBuilderQueryResponse>> Handle(
        GetSectorSubjectAreaForRolloverQueryBuilderQuery request,
        CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetSectorSubjectAreaForRolloverQueryBuilderQueryResponse>();

        try
        {
            var result = await _apiClient.PostWithResponseCode<GetSectorSubjectAreaForRolloverQueryBuilderQueryResponse>(new GetSectorSubjectAreaForRolloverQueryBuilderApiRequest(request.Filters));
            
            if (result is null)
            {
                response.Success = true;
                response.Value.SectorSubjectAreas = Enumerable.Empty<SectorSubjectArea>();
                
                return response;
            }

            response.Value.SectorSubjectAreas = result.SectorSubjectAreas.Select(o => SectorSubjectArea.FromFullCode(o.Code));
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