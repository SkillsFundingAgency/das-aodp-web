using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview.FundingApproval;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Models.ApplicationsReview;

public class QfauFundingReviewOutcomeOfferDetailsViewModelTests
{
    [Fact]
    public void Map_OrdersDetailsByFundingOfferName_CaseInsensitive_AndPreservesDetails()
    {
        var zebraOfferId = Guid.NewGuid();
        var alphaOfferId = Guid.NewGuid();
        var middleOfferId = Guid.NewGuid();

        var response = new GetFeedbackForApplicationReviewByIdQueryResponse
        {
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

        var result = QfauFundingReviewOutcomeOfferDetailsViewModel.Map(response, offers);

        Assert.Equal([alphaOfferId, middleOfferId, zebraOfferId], result.Details.Select(detail => detail.FundingOfferId));
        Assert.Equal(["Alpha comments", "Middle comments", "Zebra comments"], result.Details.Select(detail => detail.Comments));
        Assert.Equal(new DateOnly?[] { new(2026, 1, 1), new(2026, 2, 1), new(2026, 3, 1) }, result.Details.Select(detail => detail.StartDate));
        Assert.Equal(new DateOnly?[] { new(2026, 1, 31), new(2026, 2, 28), new(2026, 3, 31) }, result.Details.Select(detail => detail.EndDate));
    }

    [Fact]
    public void Map_HandlesMissingNullOrEmptyOfferNames()
    {
        var missingOfferId = Guid.NewGuid();
        var nullNameOfferId = Guid.NewGuid();
        var emptyNameOfferId = Guid.NewGuid();

        var response = new GetFeedbackForApplicationReviewByIdQueryResponse
        {
            FundedOffers =
            [
                new() { FundingOfferId = missingOfferId, Comments = "Missing offer name comments" },
                new() { FundingOfferId = nullNameOfferId, Comments = "Null offer name comments" },
                new() { FundingOfferId = emptyNameOfferId, Comments = "Empty offer name comments" }
            ]
        };

        var offers = new GetFundingOffersQueryResponse
        {
            Offers =
            [
                new() { Id = nullNameOfferId, Name = null! },
                new() { Id = emptyNameOfferId, Name = string.Empty }
            ]
        };

        var result = QfauFundingReviewOutcomeOfferDetailsViewModel.Map(response, offers);

        Assert.Equal(3, result.Details.Count);
        Assert.Contains(result.Details, detail => detail.FundingOfferId == missingOfferId && detail.Comments == "Missing offer name comments");
        Assert.Contains(result.Details, detail => detail.FundingOfferId == nullNameOfferId && detail.Comments == "Null offer name comments");
        Assert.Contains(result.Details, detail => detail.FundingOfferId == emptyNameOfferId && detail.Comments == "Empty offer name comments");
    }
}
