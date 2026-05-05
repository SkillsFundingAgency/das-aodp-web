using SFA.DAS.AODP.Domain.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Domain.Files
{
    [ExcludeFromCodeCoverage]
    public class GetFileMetadataApiRequest:IPostApiRequest
    {
        public string PostUrl => $"api/files";
        public object Data { get; set; }
    }
}
