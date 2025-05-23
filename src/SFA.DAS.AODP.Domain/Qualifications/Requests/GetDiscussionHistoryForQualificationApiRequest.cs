﻿using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests
{
    public class GetDiscussionHistoryForQualificationApiRequest : IGetApiRequest
    {
        private readonly string _qualificationReference;

        public GetDiscussionHistoryForQualificationApiRequest(string qualificationReference)
        {
            _qualificationReference = qualificationReference;
        }

        public string GetUrl => $"api/qualifications/{_qualificationReference}/qualificationdiscussionhistories";
    }
}
