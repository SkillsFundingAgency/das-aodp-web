using SFA.DAS.AODP.Domain.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Domain.Rollover;

[ExcludeFromCodeCoverage]
public class ValidateRolloverExtensionApiRequest : IPostMultipartFormDataApiRequest
{
    public string PostUrl => "api/rollover/validaterolloverextension";

    public object Data { get; set; }

    public IEnumerable<KeyValuePair<string, string>> FormData => MultipartFormDataMapper.Map(Data);
}
