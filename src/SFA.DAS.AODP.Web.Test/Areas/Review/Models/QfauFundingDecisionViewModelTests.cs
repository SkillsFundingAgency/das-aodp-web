using SFA.DAS.AODP.Models.Application;
using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview.FundingApproval;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Models.ApplicationsReview;

public class QfauFundingDecisionViewModelTests
{
    [Fact]
    public void Map_MapsBasicFields()
    {
        var applicationReviewId = Guid.NewGuid();

        var response = new GetQfauFeedbackForApplicationReviewConfirmationQueryResponse
        {
            Status = nameof(ApplicationStatus.Approved),
            Comments = "Test comments",
            FundedOffers = []
        };

        var offers = new GetFundingOffersQueryResponse
        {
            Offers = []
        };

        var result = QfauFundingDecisionViewModel.Map(applicationReviewId, response, offers);

        Assert.Equal(applicationReviewId, result.ApplicationReviewId);
        Assert.Equal("Test comments", result.Comments);
        Assert.Equal(ApplicationStatus.Approved, result.Status);
    }

    [Fact]
    public void Map_MapsFundedOffers()
    {
        var fundingOfferId = Guid.NewGuid();

        var response = new GetQfauFeedbackForApplicationReviewConfirmationQueryResponse
        {
            Status = nameof(ApplicationStatus.Approved),
            FundedOffers =
            [
                new GetQfauFeedbackForApplicationReviewConfirmationQueryResponse.Funding
                {
                    FundingOfferId = fundingOfferId,
                    Comments = "Offer comments",
                    StartDate = new DateOnly(2025, 1, 1),
                    EndDate = new DateOnly(2025, 12, 31)
                }
            ]
        };

        var offers = new GetFundingOffersQueryResponse
        {
            Offers = []
        };

        var result = QfauFundingDecisionViewModel.Map(Guid.NewGuid(), response, offers);

        Assert.Single(result.OfferFundingDetails);
        Assert.Equal(fundingOfferId, result.OfferFundingDetails[0].FundingOfferId);
        Assert.Equal("Offer comments", result.OfferFundingDetails[0].Comments);
        Assert.Equal(new DateOnly(2025, 1, 1), result.OfferFundingDetails[0].StartDate);
        Assert.Equal(new DateOnly(2025, 12, 31), result.OfferFundingDetails[0].EndDate);
    }

    [Fact]
    public void Map_MapsFundingOffers()
    {
        var offer1Id = Guid.NewGuid();
        var offer2Id = Guid.NewGuid();

        var response = new GetQfauFeedbackForApplicationReviewConfirmationQueryResponse
        {
            Status = nameof(ApplicationStatus.Approved),
            FundedOffers = []
        };

        var offers = new GetFundingOffersQueryResponse
        {
            Offers =
            [
                new GetFundingOffersQueryResponse.FundingOffer
                {
                    Id = offer1Id,
                    Name = "Offer 1"
                },
                new GetFundingOffersQueryResponse.FundingOffer
                {
                    Id = offer2Id,
                    Name = "Offer 2"
                }
            ]
        };

        var result = QfauFundingDecisionViewModel.Map(Guid.NewGuid(), response, offers);

        Assert.Equal(2, result.FundingOffers.Count);
        Assert.Contains(result.FundingOffers, x => x.Id == offer1Id && x.Name == "Offer 1");
        Assert.Contains(result.FundingOffers, x => x.Id == offer2Id && x.Name == "Offer 2");
    }

    [Fact]
    public void Map_MapsRelatedQualification()
    {
        var response = new GetQfauFeedbackForApplicationReviewConfirmationQueryResponse
        {
            Status = nameof(ApplicationStatus.Approved),
            FundedOffers = [],
            RelatedQualification = new GetQfauFeedbackForApplicationReviewConfirmationQueryResponse.Qualification
            {
                Qan = "12345678",
                Name = "Qualification name",
                Status = ProcessStatusLookup.DecisionRequired.Name
            }
        };

        var offers = new GetFundingOffersQueryResponse
        {
            Offers = []
        };

        var result = QfauFundingDecisionViewModel.Map(Guid.NewGuid(), response, offers);

        Assert.NotNull(result.RelatedQualification);
        Assert.Equal("12345678", result.RelatedQualification.Qan);
        Assert.Equal("Qualification name", result.RelatedQualification.Name);
        Assert.Equal(ProcessStatusLookup.DecisionRequired.Name, result.RelatedQualification.Status);
    }

    [Fact]
    public void Map_StatusNotApprovedWithFundedOffers_AddsMessage_AndDisablesSubmit()
    {
        var response = new GetQfauFeedbackForApplicationReviewConfirmationQueryResponse
        {
            Status = nameof(ApplicationStatus.NotApproved),
            FundedOffers =
            [
                new GetQfauFeedbackForApplicationReviewConfirmationQueryResponse.Funding
                {
                    FundingOfferId = Guid.NewGuid()
                }
            ]
        };

        var offers = new GetFundingOffersQueryResponse
        {
            Offers = []
        };

        var result = QfauFundingDecisionViewModel.Map(Guid.NewGuid(), response, offers);

        Assert.False(result.CanSubmit);
        Assert.Single(result.Messages);
        Assert.Equal(FundingDecisionMessages.NotApprovedWithOffers, result.Messages[0]);
    }

    [Fact]
    public void Map_InvalidStatus_AddsMessage_AndDisablesSubmit()
    {
        var response = new GetQfauFeedbackForApplicationReviewConfirmationQueryResponse
        {
            Status = "InvalidStatus",
            FundedOffers = []
        };

        var offers = new GetFundingOffersQueryResponse
        {
            Offers = []
        };

        var result = QfauFundingDecisionViewModel.Map(Guid.NewGuid(), response, offers);

        Assert.False(result.CanSubmit);
        Assert.Single(result.Messages);
        Assert.Equal(FundingDecisionMessages.InvalidStatus, result.Messages[0]);
    }

    [Fact]
    public void Map_ApprovedWithoutRelatedQualification_AddsMessage_AndDisablesSubmit()
    {
        var response = new GetQfauFeedbackForApplicationReviewConfirmationQueryResponse
        {
            Status = nameof(ApplicationStatus.Approved),
            FundedOffers = [],
            RelatedQualification = null
        };

        var offers = new GetFundingOffersQueryResponse
        {
            Offers = []
        };

        var result = QfauFundingDecisionViewModel.Map(Guid.NewGuid(), response, offers);

        Assert.False(result.CanSubmit);
        Assert.Single(result.Messages);
        Assert.Equal(FundingDecisionMessages.MissingQualification, result.Messages[0]);
    }

    [Theory]
    [MemberData(nameof(QualificationStatusData))]
    public void Map_InvalidQualificationStatus_AddsMessage_AndDisablesSubmit(string qualificationStatus)
    {
        var response = new GetQfauFeedbackForApplicationReviewConfirmationQueryResponse
        {
            Status = nameof(ApplicationStatus.Approved),
            FundedOffers = [],
            RelatedQualification = new GetQfauFeedbackForApplicationReviewConfirmationQueryResponse.Qualification
            {
                Qan = "12345678",
                Name = "Qualification name",
                Status = qualificationStatus
            }
        };

        var offers = new GetFundingOffersQueryResponse
        {
            Offers = []
        };

        var result = QfauFundingDecisionViewModel.Map(Guid.NewGuid(), response, offers);

        Assert.False(result.CanSubmit);
        Assert.Single(result.Messages);
        Assert.Equal(FundingDecisionMessages.InvalidQualificationStatus(qualificationStatus), result.Messages[0]);
    }

    [Fact]
    public void Map_ValidData_AddsNoMessages_AndEnablesSubmit()
    {
        var response = new GetQfauFeedbackForApplicationReviewConfirmationQueryResponse
        {
            Status = nameof(ApplicationStatus.Approved),
            FundedOffers = [],
            RelatedQualification = new GetQfauFeedbackForApplicationReviewConfirmationQueryResponse.Qualification
            {
                Qan = "12345678",
                Name = "Qualification name",
                Status = ProcessStatusLookup.DecisionRequired.Name
            }
        };

        var offers = new GetFundingOffersQueryResponse
        {
            Offers = []
        };

        var result = QfauFundingDecisionViewModel.Map(Guid.NewGuid(), response, offers);

        Assert.True(result.CanSubmit);
        Assert.Empty(result.Messages);
    }

    [Fact]
    public void Map_WhenMoreThanOneConditionCouldMatch_OnlyAddsFirstMessage()
    {
        var response = new GetQfauFeedbackForApplicationReviewConfirmationQueryResponse
        {
            Status = nameof(ApplicationStatus.NotApproved),
            FundedOffers =
            [
                new GetQfauFeedbackForApplicationReviewConfirmationQueryResponse.Funding
                {
                    FundingOfferId = Guid.NewGuid()
                }
            ],
            RelatedQualification = new GetQfauFeedbackForApplicationReviewConfirmationQueryResponse.Qualification
            {
                Qan = "12345678",
                Name = "Qualification name",
                Status = ProcessStatusLookup.Rejected.Name
            }
        };

        var offers = new GetFundingOffersQueryResponse
        {
            Offers = []
        };

        var result = QfauFundingDecisionViewModel.Map(Guid.NewGuid(), response, offers);

        Assert.Single(result.Messages);
        Assert.Equal(FundingDecisionMessages.NotApprovedWithOffers, result.Messages[0]);
    }

    public static IEnumerable<string[]> QualificationStatusData =>
    [
        [ProcessStatusLookup.Rejected.Name],
        [ProcessStatusLookup.NoActionRequired.Name]
    ];
}