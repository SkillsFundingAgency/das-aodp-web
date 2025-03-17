﻿public class GetFeedbackForQualificationFundingByIdQueryResponse
{
    public Guid QualificationVersionId { get; set; }
    public string? QualificationReference { get; set; }
    public string Status { get; set; }
    public string? Comments { get; set; }

    public List<QualificationFunding> QualificationFundedOffers { get; set; } = new();

    public class QualificationFunding
    {
        public Guid Id { get; set; }
        public Guid FundingOfferId { get; set; }
        public string FundedOfferName { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Comments { get; set; }
    }

}