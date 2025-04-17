public class GetQfauFeedbackForApplicationReviewConfirmationQueryResponse
{
    public Qualification? RelatedQualification { get; set; }
    public string? Owner { get; set; }
    public string Status { get; set; }
    public string? Comments { get; set; }

    public List<Funding> FundedOffers { get; set; } = new();

    public class Funding
    {
        public Guid Id { get; set; }
        public Guid FundingOfferId { get; set; }
        public string FundedOfferName { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Comments { get; set; }
    }

    public class Qualification
    {
        public string? Qan { get; set; }
        public string? Status { get; set; }
        public string? Name { get; set; }
    }
}
