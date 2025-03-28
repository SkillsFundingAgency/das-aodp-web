using SFA.DAS.AODP.Common.Extensions;
using SFA.DAS.AODP.Domain.Interfaces;
using System.Collections.Specialized;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests
{
    public class GetNewQualificationsApiRequest : IGetApiRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public string? Name { get; set; }
        public string? Organisation { get; set; }
        public string? QAN { get; set; }
        public Guid? ProcessStatusId { get; set; }

        public string BaseUrl = "api/qualifications";        

        public string GetUrl
        {
            get
            {                
                var queryParams = new NameValueCollection()
                {
                    { "Status", "New" },
                };

                if (Skip >= 0)
                {
                    queryParams.Add("Skip", Skip.ToString());
                }

                if (Take > 0)
                {
                    queryParams.Add("Take", Take.ToString());
                }

                if (!string.IsNullOrWhiteSpace(Name))
                {
                    queryParams.Add("Name", Name);
                }

                if (!string.IsNullOrWhiteSpace(Organisation))
                {
                    queryParams.Add("Organisation", Organisation);
                }

                if (!string.IsNullOrWhiteSpace(QAN))
                {
                    queryParams.Add("QAN", QAN);
                }

                if (ProcessStatusId.HasValue)
                {
                    queryParams.Add("ProcessStatusId", ProcessStatusId.Value.ToString());
                }

                var uri = BaseUrl.AttachParameters(queryParams);
                return uri.ToString();
            }
        }
    }
}