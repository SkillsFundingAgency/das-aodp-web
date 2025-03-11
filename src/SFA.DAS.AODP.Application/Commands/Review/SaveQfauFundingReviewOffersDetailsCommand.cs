﻿using MediatR;
using SFA.DAS.AODP.Application;

public class SaveQfauFundingReviewOffersDetailsCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public Guid ApplicationReviewId { get; set; }

    public List<OfferFundingDetails> Details { get; set; } = new();

    public class OfferFundingDetails
    {
        public Guid FundingOfferId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Comments { get; set; }
    }
}
