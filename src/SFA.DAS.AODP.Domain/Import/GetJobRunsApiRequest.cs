using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Import
{
    public class GetJobRunsApiRequest : IGetApiRequest
    {
        public string? JobName { get; set; }     

        public string GetUrl
        {
            get
            {
                return $"api/job/{JobName}/runs";
            }
        }
    }
}
