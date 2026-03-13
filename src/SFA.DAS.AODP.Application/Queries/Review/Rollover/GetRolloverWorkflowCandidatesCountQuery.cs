using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

[ExcludeFromCodeCoverage]
public record GetRolloverWorkflowCandidatesCountQuery() : IRequest<BaseMediatrResponse<GetRolloverWorkflowCandidatesCountQueryResponse>>;