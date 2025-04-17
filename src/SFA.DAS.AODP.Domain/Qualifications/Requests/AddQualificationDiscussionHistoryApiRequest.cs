using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests
{
    public class AddQualificationDiscussionHistoryApiRequest : IPostApiRequest
    {

        public AddQualificationDiscussionHistoryApiRequest(object data)
        {
            Data = data;
        }

        public string PostUrl => $"api/qualifications/qualificationdiscussionhistory";

        public object Data { get; set; }
    }
}