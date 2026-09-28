namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class GetAwardingOrganisationsForRolloverQueryBuilderQuery(RolloverQueryBuilderAwardingOrganisationsRequest filters)
    : IRequest<BaseMediatrResponse<GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse>>
{
    public RolloverQueryBuilderAwardingOrganisationsRequest Filters { get; } = filters;
}