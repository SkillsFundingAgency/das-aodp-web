﻿using SFA.DAS.AODP.Domain.Interfaces;

public class SaveQfauFundingReviewDecisionApiRequest(Guid applicationReviewId) : IPutApiRequest
{
    public string PutUrl => $"api/application-reviews/{applicationReviewId}/qfau-decision";

    public object Data { get; set; }
}