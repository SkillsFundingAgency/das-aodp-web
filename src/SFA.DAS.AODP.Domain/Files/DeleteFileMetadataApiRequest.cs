using SFA.DAS.AODP.Domain.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Domain.Files
{
    [ExcludeFromCodeCoverage]
    public class  DeleteFileMetadataApiRequest:IPostApiRequest
    {
        public string PostUrl => $"api/files/create";
        public object Data { get; set; }
    }
}
