namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class GetQualificationVersionsForRolloverQueryBuilderQuery(RolloverQueryBuilderRequest filters)
    : IRequest<BaseMediatrResponse<GetQualificationVersionsForRolloverQueryBuilderQueryResponse>>
{
    public RolloverQueryBuilderRequest Filters { get; } = filters;
}
