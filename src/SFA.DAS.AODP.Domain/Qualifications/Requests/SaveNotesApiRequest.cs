using SFA.DAS.AODP.Common.Extensions;
using SFA.DAS.AODP.Domain.Interfaces;
using System;
using System.Collections.Specialized;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests
{
    public class SaveNotesApiRequest : IGetApiRequest
    {
        public Guid QualificationReferenceId { get; set; }

        public Guid ActionTypeId { get; set; }

        public string Notes { get; set; }
        public string BaseUrl = "api/qualifications/{qualificationReferenceId}/Save";

        public string GetUrl
        {
            get
            {
                var queryParams = new NameValueCollection();

                if (!string.IsNullOrWhiteSpace(QualificationReferenceId.ToString()))
                {
                    queryParams.Add("qualificationReferenceId", QualificationReferenceId.ToString());
                }

                if (!string.IsNullOrWhiteSpace(ActionTypeId.ToString()))
                {
                    queryParams.Add("actionTypeId", ActionTypeId.ToString());
                }

                if (!string.IsNullOrWhiteSpace(Notes))
                {
                    queryParams.Add("notes", Notes);
                }

                var uri = BaseUrl.AttachParameters(queryParams);
                return uri.ToString();
            }
        }
      

    }
}