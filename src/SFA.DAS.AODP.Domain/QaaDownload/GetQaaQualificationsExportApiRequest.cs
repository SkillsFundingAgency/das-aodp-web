using SFA.DAS.AODP.Domain.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Domain.QaaDownload;

[ExcludeFromCodeCoverage]
public class GetQaaQualificationsExportApiRequest : IGetApiRequest
{
    public string CurrentUsername { get; set; } = string.Empty;
    public string GetUrl => $"api/qaa/download?username={Uri.EscapeDataString(CurrentUsername)}";
}
