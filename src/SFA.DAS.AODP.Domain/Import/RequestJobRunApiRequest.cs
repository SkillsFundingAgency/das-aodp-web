

using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Import
{
    public class RequestJobRunApiRequest : IPostApiRequest
    {

        public RequestJobRunApiRequest()
        {
        }

        public object Data { get; set; }

        public string PostUrl => $"/api/job/requestrun";
    }
}
