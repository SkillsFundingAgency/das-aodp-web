using SFA.DAS.AODP.Common.Extensions;
using SFA.DAS.AODP.Domain.Interfaces;
using System.Collections.Specialized;

namespace SFA.DAS.AODP.Domain.Rollover;

public class GetRolloverWorkflowCandidatesApiRequest : IGetApiRequest
{
    public int? Skip { get; set; }
    public int? Take { get; set; }

    public GetRolloverWorkflowCandidatesApiRequest(int? skip, int? take)
    {
        Skip = skip;
        Take = take;
    }


    private string BaseUrl = "api/rollover/workflowcandidates";

    public string GetUrl
    {
        get
        {
            var queryParams = new NameValueCollection();

            if (Skip >= 0)
            {
                queryParams.Add("Skip", Skip.ToString());
            }

            if (Take > 0)
            {
                queryParams.Add("Take", Take.ToString());
            }

            var uri = BaseUrl.AttachParameters(queryParams);

            return uri.ToString();
        }
    }
}
