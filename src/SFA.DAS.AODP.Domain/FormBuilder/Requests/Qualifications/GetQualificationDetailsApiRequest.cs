using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.FormBuilder.Requests.Qualifications
{
    public class GetQualificationDetailsApiRequest : IGetApiRequest
    {
        private readonly int _id;

        public GetQualificationDetailsApiRequest(int id)
        {
            _id = id;
        }

        public string GetUrl => $"api/new-qualifications/{_id}";
    }
}
