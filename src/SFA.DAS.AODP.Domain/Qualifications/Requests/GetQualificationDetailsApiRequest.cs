﻿using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests
{
    public class GetQualificationDetailsApiRequest : IGetApiRequest
    {
        private readonly string _qualificationReference;

        public GetQualificationDetailsApiRequest(string qualificationReference)
        {
            _qualificationReference = qualificationReference;
        }

        public string GetUrl => $"api/new-qualifications/{_qualificationReference}";
    }
}
