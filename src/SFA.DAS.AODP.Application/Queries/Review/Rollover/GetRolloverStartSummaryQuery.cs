using MediatR;
using System.Diagnostics.CodeAnalysis;
namespace SFA.DAS.AODP.Application.Queries.Rollover
{
    [ExcludeFromCodeCoverage]
    public class GetRolloverStartSummaryQuery : IRequest<BaseMediatrResponse<GetRolloverStartSummaryQueryResponse>>
    { 
    }
}