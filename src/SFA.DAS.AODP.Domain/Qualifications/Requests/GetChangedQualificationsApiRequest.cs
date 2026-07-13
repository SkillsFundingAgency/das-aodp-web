using SFA.DAS.AODP.Common.Extensions;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Models;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests
{
    [ExcludeFromCodeCoverage]
    public class GetChangedQualificationsApiRequest : IGetApiRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public string? Name { get; set; }
        public string? Organisation { get; set; }
        public string? QAN { get; set; }
        public ProcessStatusFilter? ProcessStatusFilter { get; set; }
        public List<AgeGroup> AgeGroups { get; set; } = new();

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

                if (ProcessStatusFilter?.ProcessStatusIds?.Count > 0)
                {
                    foreach (var id in ProcessStatusFilter.ProcessStatusIds)
                    {
                        queryParams.Add("ProcessStatusFilter", id.ToString());
                    }
                }

                if (AgeGroups?.Count > 0)
                {
                    foreach (var age in AgeGroups)
                    {
                        queryParams.Add("AgeGroups", ((int)age).ToString());
                    }
                }

                var uri = BaseUrl.AttachMultiValueParameters(queryParams);

                return uri.ToString();
            }
        }
    }
}