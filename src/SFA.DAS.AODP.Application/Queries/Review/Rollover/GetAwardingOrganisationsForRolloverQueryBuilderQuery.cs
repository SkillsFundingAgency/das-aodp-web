using MediatR;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class GetAwardingOrganisationsForRolloverQueryBuilderQuery(RolloverQueryBuilderRequest filters)
    : IRequest<BaseMediatrResponse<GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse>>
{
    public RolloverQueryBuilderRequest Filters { get; } = filters;
}
