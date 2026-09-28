using MediatR;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class GetSectorSubjectAreaForRolloverQueryBuilderQuery(RolloverQueryBuilderSectorSubjectAreaRequest filters)
    : IRequest<BaseMediatrResponse<GetSectorSubjectAreaForRolloverQueryBuilderQueryResponse>>
{
    public RolloverQueryBuilderSectorSubjectAreaRequest Filters { get; } = filters;
}