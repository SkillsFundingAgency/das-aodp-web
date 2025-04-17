using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests
{
    public class UpdateQualificationStatusApiRequest : IPostApiRequest
    {

        public UpdateQualificationStatusApiRequest(object data)
        {
            Data = data;
        }

        public string PostUrl => $"api/qualifications/qualificationstatus";

        public object Data { get; set; }
    }
}