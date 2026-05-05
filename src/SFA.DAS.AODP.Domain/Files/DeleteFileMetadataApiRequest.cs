using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Files
{
    public class  DeleteFileMetadataApiRequest:IPostApiRequest
    {
        public string PostUrl => $"api/files/create";
        public object Data { get; set; }
    }
}
