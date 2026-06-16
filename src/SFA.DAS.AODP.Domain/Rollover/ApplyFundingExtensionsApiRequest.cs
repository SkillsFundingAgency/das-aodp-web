using SFA.DAS.AODP.Domain.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Domain.Rollover;

[ExcludeFromCodeCoverage]
public class ApplyFundingExtensionsApiRequest : IPostApiRequest
{
    public string PostUrl => "api/rollover/applyrolloverextension";

    public object Data { get; set; }
}
