using MediatR;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class GetTypesForRolloverQueryBuilderQuery(RolloverQueryBuilderTypesRequest filters)
    : IRequest<BaseMediatrResponse<GetTypesForRolloverQueryBuilderQueryResponse>>
{
    public RolloverQueryBuilderTypesRequest Filters { get; } = filters;
}