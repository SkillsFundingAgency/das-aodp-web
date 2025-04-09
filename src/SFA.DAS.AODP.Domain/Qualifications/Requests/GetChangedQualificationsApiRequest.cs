using SFA.DAS.AODP.Common.Extensions;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Models;
using System.Collections.Specialized;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests
{
    public class GetChangedQualificationsApiRequest : IGetApiRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public string? Name { get; set; }
        public string? Organisation { get; set; }
        public string? QAN { get; set; }
        public ProcessStatusFilter? ProcessStatusFilter { get; set; }

        public string BaseUrl = "api/qualifications";

        public string GetUrl
        {
            get
            {
                var queryParams = new NameValueCollection()
                {
                    { "Status", "Changed" },
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

                if (ProcessStatusFilter != null && ProcessStatusFilter.ProcessStatusIds.Any())
                {
                    var ids = string.Join(",", ProcessStatusFilter.ProcessStatusIds);
                    ids = Uri.EscapeDataString(ids);
                    queryParams.Add("ProcessStatusFilter", ids);
                }

                var uri = BaseUrl.AttachParameters(queryParams);
                
                return uri.ToString();
            }
        }
    }
}