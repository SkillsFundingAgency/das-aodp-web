using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Files
{
    public class GetFileMetadataApiRequest:IPostApiRequest
    {
        public string PostUrl => $"api/files";
        public object Data { get; set; }
    }
}
