using SFA.DAS.AODP.Domain.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Domain.Rollover;

[ExcludeFromCodeCoverage]
public class ValidateFundingExtensionCandidatesApiRequest : IPostApiRequest
{
    public string PostUrl => "api/rollover/rolloverextensionvalidation";

    public object Data { get; set; }
}
