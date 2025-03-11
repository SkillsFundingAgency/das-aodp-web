﻿using SFA.DAS.AODP.Domain.Interfaces;

public class GetApplicationReviewByIdApiRequest : IGetApiRequest
{
    public Guid ApplicationReviewId { get; set; }

    public GetApplicationReviewByIdApiRequest(Guid applicationReviewId)
    {
        ApplicationReviewId = applicationReviewId;
    }

    public string GetUrl => $"api/application-reviews/{ApplicationReviewId}";

}
