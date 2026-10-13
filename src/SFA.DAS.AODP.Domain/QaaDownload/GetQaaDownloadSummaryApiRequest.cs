using SFA.DAS.AODP.Domain.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Domain.QaaDownload;

[ExcludeFromCodeCoverage]
public class GetQaaDownloadSummaryApiRequest : IGetApiRequest
{
    public string GetUrl => "api/qaa/download-summary";
}
