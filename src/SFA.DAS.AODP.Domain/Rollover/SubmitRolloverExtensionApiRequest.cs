using SFA.DAS.AODP.Domain.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Domain.Rollover;

[ExcludeFromCodeCoverage]
public class SubmitRolloverExtensionApiRequest : IPostApiRequest
{
    public string PostUrl => "api/rollover/submitrolloverextension";

    public object Data { get; set; }
}
