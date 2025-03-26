using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Import
{
    public class GetJobByNameApiRequest : IGetApiRequest
    {
        private readonly string Name;

        public GetJobByNameApiRequest(string name)
        {
            Name = name;
        }

        public string GetUrl => $"api/job/?name={Name}";
    }
}
