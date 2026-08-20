using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Application.Queries.QaaDownload
{
    [ExcludeFromCodeCoverage]
    public class GetQaaQualificationsExportQuery : IRequest<BaseMediatrResponse<GetQaaQualificationsExportQueryResponse>>
    {
        public string CurrentUsername { get; set; } = string.Empty;
    }
}
