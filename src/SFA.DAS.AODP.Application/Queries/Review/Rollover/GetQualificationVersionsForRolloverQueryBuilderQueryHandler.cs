namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class GetQualificationVersionsForRolloverQueryBuilderQueryHandler(IApiClient apiClient)
    : IRequestHandler<GetQualificationVersionsForRolloverQueryBuilderQuery, BaseMediatrResponse<GetQualificationVersionsForRolloverQueryBuilderQueryResponse>>
{
    public async Task<BaseMediatrResponse<GetQualificationVersionsForRolloverQueryBuilderQueryResponse>> Handle(
        GetQualificationVersionsForRolloverQueryBuilderQuery request,
        CancellationToken cancellationToken)
    {
        var result = await apiClient.PostWithResponseCodeAsJsonFile<GetQualificationVersionsForRolloverQueryBuilderQueryResponse>(
            new GetQualificationVersionsForRolloverQueryBuilderApiRequest(request.Filters));

        return new BaseMediatrResponse<GetQualificationVersionsForRolloverQueryBuilderQueryResponse>
        {
            Success = true,
            Value = result ?? new GetQualificationVersionsForRolloverQueryBuilderQueryResponse()
        };
    }
}
