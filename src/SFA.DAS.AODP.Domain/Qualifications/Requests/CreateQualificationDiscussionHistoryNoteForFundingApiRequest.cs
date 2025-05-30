﻿using SFA.DAS.AODP.Domain.Interfaces;

public class CreateQualificationDiscussionHistoryNoteForFundingApiRequest : IPutApiRequest
{
    public Guid QualificationVersionId { get; set; }
    public string PutUrl => $"api/qualifications/{QualificationVersionId}/funding-offers-history-note";
    public object Data { get; set; }
}
