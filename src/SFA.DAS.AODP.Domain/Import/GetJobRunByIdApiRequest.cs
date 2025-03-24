using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Import
{
    public class GetJobRunByIdApiRequest : IGetApiRequest
    {
        private readonly Guid Id;

        public GetJobRunByIdApiRequest(Guid id)
        {
            Id = id;
        }

        public string GetUrl => $"api/job/jobruns/{Id}";
    }
}
