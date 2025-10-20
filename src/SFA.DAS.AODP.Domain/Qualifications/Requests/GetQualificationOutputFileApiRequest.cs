using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests
{
    public class GetQualificationOutputFileApiRequest : IGetApiRequest
    {
        public string CurrentUsername { get; set; } = string.Empty;
        public string GetUrl => "api/qualifications/outputfile";

        public GetQualificationOutputFileApiRequest(string currentUsername)
        {
            CurrentUsername = currentUsername;
        }
    }
}
