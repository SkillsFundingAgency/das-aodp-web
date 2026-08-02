using SFA.DAS.AODP.Domain.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Domain.Rollover;

[ExcludeFromCodeCoverage]
public class ValidateRolloverExtensionApiRequest : IPostMultipartJsonFileApiRequest
{
    public string PostUrl => "api/rollover/validaterolloverextension";

    public object Data { get; set; }
}
