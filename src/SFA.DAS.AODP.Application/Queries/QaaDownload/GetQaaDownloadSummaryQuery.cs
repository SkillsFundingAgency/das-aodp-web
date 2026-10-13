using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Application.Queries.QaaDownload
{
    [ExcludeFromCodeCoverage]
    public class GetQaaDownloadSummaryQuery : IRequest<BaseMediatrResponse<GetQaaDownloadSummaryQueryResponse>>
    {
    }
}
