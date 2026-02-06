using SFA.DAS.AODP.Common.Extensions;
using SFA.DAS.AODP.Domain.Interfaces;
using System.Collections.Specialized;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests;

public class GetQualificationsApiRequest : IGetApiRequest
{
    public string SearchTerm { get; set; }

    // Pagination
    public int? Skip { get; set; } = 0;
    public int? Take { get; set; } = 25;

    public string BaseUrl = "api/qualifications/GetMatchingQualifications";

    public GetQualificationsApiRequest(string searchTerm, int? skip, int? take)
    {
        SearchTerm = searchTerm;
        Skip = skip;
        Take = take;
    }

    public string GetUrl
    {
        get
        {
            var queryParams = new NameValueCollection()
                {
                    { "SearchTerm", SearchTerm },
                };

            if (Skip.HasValue)
            {
                queryParams.Add("Skip", Skip.ToString());
            }

            if (Take.HasValue)
            {
                queryParams.Add("Take", Take.ToString());
            }

            var url = BaseUrl.AttachParameters(queryParams);

            return url;
        }
    }
}
