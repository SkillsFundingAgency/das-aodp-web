using SFA.DAS.AODP.Models.Application;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview.FundingApproval;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Models.ApplicationsReview;

public class QfauFundingReviewOutcomeSummaryViewModelTests
{
    [Fact]
    public void Map_OrdersDetailsByFundingOfferName_CaseInsensitive_AndPreservesDetails()
    {
        var zebraOfferId = Guid.NewGuid();
        var alphaOfferId = Guid.NewGuid();
        var middleOfferId = Guid.NewGuid();

        var response = new GetFeedbackForApplicationReviewByIdQueryResponse
        {
            Status = ApplicationStatus.Approved.ToString(),
            FundedOffers =
            [
                new()
                {
                    FundingOfferId = zebraOfferId,
                    StartDate = new DateOnly(2026, 3, 1),
                    EndDate = new DateOnly(2026, 3, 31),
                    Comments = "Zebra comments"
                },
                new()
                {
                    FundingOfferId = alphaOfferId,
                    StartDate = new DateOnly(2026, 1, 1),
                    EndDate = new DateOnly(2026, 1, 31),
                    Comments = "Alpha comments"
                },
                new()
                {
                    FundingOfferId = middleOfferId,
                    StartDate = new DateOnly(2026, 2, 1),
                    EndDate = new DateOnly(2026, 2, 28),
                    Comments = "Middle comments"
                }
            ]
        };

        var offers = new GetFundingOffersQueryResponse
        {
            Offers =
            [
                new() { Id = zebraOfferId, Name = "Zebra offer" },
                new() { Id = alphaOfferId, Name = "alpha offer" },
                new() { Id = middleOfferId, Name = "Middle offer" }
            ]
        };

        var result = QfauFundingReviewOutcomeSummaryViewModel.Map(response, offers);

        Assert.Equal([alphaOfferId, middleOfferId, zebraOfferId], result.OfferFundingDetails.Select(detail => detail.FundingOfferId));
        Assert.Equal(["Alpha comments", "Middle comments", "Zebra comments"], result.OfferFundingDetails.Select(detail => detail.Comments));
        Assert.Equal(new DateOnly?[] { new(2026, 1, 1), new(2026, 2, 1), new(2026, 3, 1) }, result.OfferFundingDetails.Select(detail => detail.StartDate));
        Assert.Equal(new DateOnly?[] { new(2026, 1, 31), new(2026, 2, 28), new(2026, 3, 31) }, result.OfferFundingDetails.Select(detail => detail.EndDate));
    }
}
