using Moq;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;
namespace SFA.DAS.AODP.Application.UnitTests.Queries.Qualifications;

public class GetQualificationTimelineQueryHandlerTests
{
    private const string QualificationReference = "61054902";
    private const string ExistingTitle = "Existing";
    private const string ExistingNote = "Existing note";
    private const string ExistingUser = "User";
    private const string ChangeTitle = "Change";
    private const string OfqualImportUser = "OFQUAL Import";
    private const string QualificationNameOld = "Old Qualification";
    private const string QualificationNameNew = "New Qualification";
    private const string OrganisationNameOld = "Old Organisation";
    private const string OrganisationNameNew = "New Organisation";
    private const string FailedFields = "Title";
    private const string ErrorPrefixDiscussionHistory = "Failed to get qualification discussion history";
    private const string ErrorPrefixVersions = "Failed to get qualification versions";
    private const string ExceptionMessage = "boom";
    private const string EmptyQan = "";

    private const int VersionOne = 1;
    private const int VersionTwo = 2;
    private const int VersionThree = 3;

    private static readonly DateTime ExistingHistoryTimestamp = new(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime VersionOneTimestamp = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime VersionTwoTimestamp = new(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime VersionThreeTimestamp = new(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IApiClient> _apiClient = new();

    private GetQualificationTimelineQueryHandler CreateHandler()
    {
        return new GetQualificationTimelineQueryHandler(_apiClient.Object);
    }

    private static QualificationDiscussionHistoriesResponse CreateDiscussionHistoryResponse(
        params QualificationDiscussionHistory[] histories)
    {
        return new QualificationDiscussionHistoriesResponse
        {
            QualificationDiscussionHistories = histories.ToList()
        };
    }

    private static GetQualificationDetailsQueryResponse CreateQualificationWithVersionsResponse(
        params GetQualificationDetailsQueryResponse[] versions)
    {
        return new GetQualificationDetailsQueryResponse
        {
            Qual = new GetQualificationDetailsQueryResponse.Qualification
            {
                Id = Guid.NewGuid(),
                Qan = QualificationReference,
                QualificationName = QualificationNameNew,
                Versions = versions.ToList()
            }
        };
    }

    private static GetQualificationDetailsQueryResponse CreateVersion(
        int version,
        DateTime insertedTimestamp,
        string qualificationName,
        string organisationName,
        string? versionFieldChanges = null,
        string? fundingEligibilityFailedFields = null,
        bool? eligibleForFunding = true)
    {
        return new GetQualificationDetailsQueryResponse
        {
            Id = Guid.NewGuid(),
            QualificationId = Guid.NewGuid(),
            Version = version,
            VersionFieldChanges = versionFieldChanges,
            FundingEligibilityFailedFields = fundingEligibilityFailedFields,
            EligibleForFunding = eligibleForFunding,
            InsertedTimestamp = insertedTimestamp,
            LastUpdatedDate = insertedTimestamp,
            Qual = new GetQualificationDetailsQueryResponse.Qualification
            {
                Id = Guid.NewGuid(),
                Qan = QualificationReference,
                QualificationName = qualificationName,
                Versions = new List<GetQualificationDetailsQueryResponse>()
            },
            Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation
            {
                Id = Guid.NewGuid(),
                NameOfqual = organisationName
            },
            Type = "Type A"
        };
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenDiscussionHistoryResultIsNull()
    {
        var handler = CreateHandler();

        _apiClient.Setup(x => x.Get<QualificationDiscussionHistoriesResponse>(
                It.IsAny<GetDiscussionHistoryForQualificationApiRequest>()))
            .ReturnsAsync(() => null!);

        var result = await handler.Handle(
            new GetQualificationTimelineQuery { QualificationReference = QualificationReference },
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value.QualificationDiscussionHistories);
            Assert.Contains(ErrorPrefixDiscussionHistory, result.ErrorMessage);
        });
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenQualificationVersionsResultIsNull()
    {
        var handler = CreateHandler();

        _apiClient.Setup(x => x.Get<QualificationDiscussionHistoriesResponse>(
                It.IsAny<GetDiscussionHistoryForQualificationApiRequest>()))
            .ReturnsAsync(CreateDiscussionHistoryResponse());

        _apiClient.Setup(x => x.Get<GetQualificationDetailsQueryResponse>(
                It.IsAny<GetQualificationDetailWithVersionsApiRequest>()))
            .ReturnsAsync(() => null!);

        var result = await handler.Handle(
            new GetQualificationTimelineQuery { QualificationReference = QualificationReference },
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value.QualificationDiscussionHistories);
            Assert.Contains(ErrorPrefixVersions, result.ErrorMessage);
        });
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenApiThrowsException()
    {
        var handler = CreateHandler();

        _apiClient.Setup(x => x.Get<QualificationDiscussionHistoriesResponse>(
                It.IsAny<GetDiscussionHistoryForQualificationApiRequest>()))
            .ThrowsAsync(new Exception(ExceptionMessage));

        var result = await handler.Handle(
            new GetQualificationTimelineQuery { QualificationReference = QualificationReference },
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value.QualificationDiscussionHistories);
            Assert.Equal(ExceptionMessage, result.ErrorMessage);
        });
    }

    [Fact]
    public async Task Handle_WithOneVersion_AndNoChangedOrFundingFields_ReturnsOnlyExistingHistory()
    {
        var handler = CreateHandler();

        var discussionHistory = CreateDiscussionHistoryResponse(
            new QualificationDiscussionHistory
            {
                Id = Guid.NewGuid(),
                QualificationId = Guid.NewGuid(),
                Title = ExistingTitle,
                Notes = ExistingNote,
                UserDisplayName = ExistingUser,
                Timestamp = ExistingHistoryTimestamp
            });

        var version = CreateVersion(
            VersionOne,
            VersionOneTimestamp,
            QualificationNameNew,
            OrganisationNameNew);

        var qualification = CreateQualificationWithVersionsResponse(version);

        _apiClient.Setup(x => x.Get<QualificationDiscussionHistoriesResponse>(
                It.IsAny<GetDiscussionHistoryForQualificationApiRequest>()))
            .ReturnsAsync(discussionHistory);

        _apiClient.Setup(x => x.Get<GetQualificationDetailsQueryResponse>(
                It.IsAny<GetQualificationDetailWithVersionsApiRequest>()))
            .ReturnsAsync(qualification);

        var result = await handler.Handle(
            new GetQualificationTimelineQuery { QualificationReference = QualificationReference },
            CancellationToken.None);

        var item = Assert.Single(result.Value!.QualificationDiscussionHistories);

        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.Equal(ExistingTitle, item.Title);
            Assert.Equal(ExistingNote, item.Notes);
            Assert.Equal(ExistingUser, item.UserDisplayName);
            Assert.Equal(ExistingHistoryTimestamp, item.Timestamp);
        });
    }

    [Fact]
    public async Task Handle_WithOneVersion_AndFundingFailedFields_AddsFundingEntry()
    {
        var handler = CreateHandler();

        var discussionHistory = CreateDiscussionHistoryResponse();

        var version = CreateVersion(
            VersionOne,
            VersionOneTimestamp,
            QualificationNameNew,
            OrganisationNameNew,
            fundingEligibilityFailedFields: FailedFields,
            eligibleForFunding: false);

        var qualification = CreateQualificationWithVersionsResponse(version);

        _apiClient.Setup(x => x.Get<QualificationDiscussionHistoriesResponse>(
                It.IsAny<GetDiscussionHistoryForQualificationApiRequest>()))
            .ReturnsAsync(discussionHistory);

        _apiClient.Setup(x => x.Get<GetQualificationDetailsQueryResponse>(
                It.IsAny<GetQualificationDetailWithVersionsApiRequest>()))
            .ReturnsAsync(qualification);

        var result = await handler.Handle(
            new GetQualificationTimelineQuery { QualificationReference = QualificationReference },
            CancellationToken.None);

        var item = Assert.Single(result.Value!.QualificationDiscussionHistories);

        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.Equal(ChangeTitle, item.Title);
            Assert.Equal(OfqualImportUser, item.UserDisplayName);
            Assert.Equal(VersionOneTimestamp, item.Timestamp);
            Assert.False(string.IsNullOrWhiteSpace(item.Notes));
        });
    }

    [Fact]
    public async Task Handle_WithMultipleVersions_AndChangedFields_AddsChangeEntry()
    {
        var handler = CreateHandler();

        var discussionHistory = CreateDiscussionHistoryResponse();

        var previousVersion = CreateVersion(
            VersionOne,
            VersionOneTimestamp,
            QualificationNameOld,
            OrganisationNameOld);

        var latestVersion = CreateVersion(
            VersionTwo,
            VersionTwoTimestamp,
            QualificationNameNew,
            OrganisationNameNew,
            versionFieldChanges: "Title,OrganisationName");

        var qualification = CreateQualificationWithVersionsResponse(latestVersion, previousVersion);

        _apiClient.Setup(x => x.Get<QualificationDiscussionHistoriesResponse>(
                It.IsAny<GetDiscussionHistoryForQualificationApiRequest>()))
            .ReturnsAsync(discussionHistory);

        _apiClient.Setup(x => x.Get<GetQualificationDetailsQueryResponse>(
                It.IsAny<GetQualificationDetailWithVersionsApiRequest>()))
            .ReturnsAsync(qualification);

        var result = await handler.Handle(
            new GetQualificationTimelineQuery { QualificationReference = QualificationReference },
            CancellationToken.None);

        var item = Assert.Single(result.Value!.QualificationDiscussionHistories);

        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.Equal(ChangeTitle, item.Title);
            Assert.Equal(OfqualImportUser, item.UserDisplayName);
            Assert.Equal(VersionTwoTimestamp, item.Timestamp);
            Assert.Contains("title changed from", item.Notes);
            Assert.Contains("organisation name changed from", item.Notes);
        });
    }

    [Fact]
    public async Task Handle_WithMultipleVersions_AndNoChangedFields_DoesNotAddChangeEntry()
    {
        var handler = CreateHandler();

        var discussionHistory = CreateDiscussionHistoryResponse();

        var previousVersion = CreateVersion(
            VersionOne,
            VersionOneTimestamp,
            QualificationNameOld,
            OrganisationNameOld);

        var latestVersion = CreateVersion(
            VersionTwo,
            VersionTwoTimestamp,
            QualificationNameNew,
            OrganisationNameNew,
            versionFieldChanges: null);

        var qualification = CreateQualificationWithVersionsResponse(latestVersion, previousVersion);

        _apiClient.Setup(x => x.Get<QualificationDiscussionHistoriesResponse>(
                It.IsAny<GetDiscussionHistoryForQualificationApiRequest>()))
            .ReturnsAsync(discussionHistory);

        _apiClient.Setup(x => x.Get<GetQualificationDetailsQueryResponse>(
                It.IsAny<GetQualificationDetailWithVersionsApiRequest>()))
            .ReturnsAsync(qualification);

        var result = await handler.Handle(
            new GetQualificationTimelineQuery { QualificationReference = QualificationReference },
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.Empty(result.Value!.QualificationDiscussionHistories);
        });
    }

    [Fact]
    public async Task Handle_WithMultipleVersions_AndMixedFundingFailedFields_AddsFundingEntriesOnlyWherePresent()
    {
        var handler = CreateHandler();

        var discussionHistory = CreateDiscussionHistoryResponse();

        var versionOne = CreateVersion(
            VersionOne,
            VersionOneTimestamp,
            QualificationNameOld,
            OrganisationNameOld);

        var versionTwo = CreateVersion(
            VersionTwo,
            VersionTwoTimestamp,
            QualificationNameNew,
            OrganisationNameNew,
            fundingEligibilityFailedFields: FailedFields,
            eligibleForFunding: false);

        var versionThree = CreateVersion(
            VersionThree,
            VersionThreeTimestamp,
            QualificationNameNew,
            OrganisationNameNew);

        var qualification = CreateQualificationWithVersionsResponse(versionThree, versionTwo, versionOne);

        _apiClient.Setup(x => x.Get<QualificationDiscussionHistoriesResponse>(
                It.IsAny<GetDiscussionHistoryForQualificationApiRequest>()))
            .ReturnsAsync(discussionHistory);

        _apiClient.Setup(x => x.Get<GetQualificationDetailsQueryResponse>(
                It.IsAny<GetQualificationDetailWithVersionsApiRequest>()))
            .ReturnsAsync(qualification);

        var result = await handler.Handle(
            new GetQualificationTimelineQuery { QualificationReference = QualificationReference },
            CancellationToken.None);

        var item = Assert.Single(result.Value!.QualificationDiscussionHistories);

        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.Equal(ChangeTitle, item.Title);
            Assert.Equal(OfqualImportUser, item.UserDisplayName);
            Assert.Equal(VersionTwoTimestamp, item.Timestamp);
            Assert.False(string.IsNullOrWhiteSpace(item.Notes));
        });
    }

    [Fact]
    public async Task Handle_WithMultipleVersions_AndMixedChangedAndFundingFields_AddsBothEntryTypes()
    {
        var handler = CreateHandler();

        var discussionHistory = CreateDiscussionHistoryResponse();

        var versionOne = CreateVersion(
            VersionOne,
            VersionOneTimestamp,
            QualificationNameOld,
            OrganisationNameOld);

        var versionTwo = CreateVersion(
            VersionTwo,
            VersionTwoTimestamp,
            QualificationNameNew,
            OrganisationNameNew,
            versionFieldChanges: "Title",
            fundingEligibilityFailedFields: FailedFields,
            eligibleForFunding: false);

        var qualification = CreateQualificationWithVersionsResponse(versionTwo, versionOne);

        _apiClient.Setup(x => x.Get<QualificationDiscussionHistoriesResponse>(
                It.IsAny<GetDiscussionHistoryForQualificationApiRequest>()))
            .ReturnsAsync(discussionHistory);

        _apiClient.Setup(x => x.Get<GetQualificationDetailsQueryResponse>(
                It.IsAny<GetQualificationDetailWithVersionsApiRequest>()))
            .ReturnsAsync(qualification);

        var result = await handler.Handle(
            new GetQualificationTimelineQuery { QualificationReference = QualificationReference },
            CancellationToken.None);

        var items = result.Value!.QualificationDiscussionHistories;

        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.Equal(2, items.Count);
            Assert.All(items, x => Assert.Equal(ChangeTitle, x.Title));
            Assert.All(items, x => Assert.Equal(OfqualImportUser, x.UserDisplayName));
            Assert.Contains(items, x => x.Notes.Contains("title changed from"));
            Assert.Contains(items, x => !string.IsNullOrWhiteSpace(x.Notes) && !x.Notes.Contains("title changed from"));
        });
    }

    [Fact]
    public async Task Handle_WithMissingIntermediateVersion_DoesNotCreateMisleadingChangeEntry()
    {
        var handler = CreateHandler();

        var discussionHistory = CreateDiscussionHistoryResponse();

        var versionOne = CreateVersion(
            VersionOne,
            VersionOneTimestamp,
            QualificationNameOld,
            OrganisationNameOld);

        var versionThree = CreateVersion(
            VersionThree,
            VersionThreeTimestamp,
            QualificationNameNew,
            OrganisationNameNew,
            versionFieldChanges: "Title");

        var qualification = CreateQualificationWithVersionsResponse(versionThree, versionOne);

        _apiClient.Setup(x => x.Get<QualificationDiscussionHistoriesResponse>(
                It.IsAny<GetDiscussionHistoryForQualificationApiRequest>()))
            .ReturnsAsync(discussionHistory);

        _apiClient.Setup(x => x.Get<GetQualificationDetailsQueryResponse>(
                It.IsAny<GetQualificationDetailWithVersionsApiRequest>()))
            .ReturnsAsync(qualification);

        var result = await handler.Handle(
            new GetQualificationTimelineQuery { QualificationReference = QualificationReference },
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.Empty(result.Value!.QualificationDiscussionHistories);
        });
    }

    [Fact]
    public async Task Handle_ReturnsEntriesOrderedByTimestampDescending()
    {
        var handler = CreateHandler();

        var existingHistory = new QualificationDiscussionHistory
        {
            Id = Guid.NewGuid(),
            QualificationId = Guid.NewGuid(),
            Title = ExistingTitle,
            Notes = ExistingNote,
            UserDisplayName = ExistingUser,
            Timestamp = ExistingHistoryTimestamp
        };

        var discussionHistory = CreateDiscussionHistoryResponse(existingHistory);

        var versionOne = CreateVersion(
            VersionOne,
            VersionOneTimestamp,
            QualificationNameOld,
            OrganisationNameOld);

        var versionTwo = CreateVersion(
            VersionTwo,
            VersionTwoTimestamp,
            QualificationNameNew,
            OrganisationNameNew,
            versionFieldChanges: "Title");

        var versionThree = CreateVersion(
            VersionThree,
            VersionThreeTimestamp,
            QualificationNameNew,
            OrganisationNameNew,
            fundingEligibilityFailedFields: FailedFields,
            eligibleForFunding: false);

        var qualification = CreateQualificationWithVersionsResponse(versionThree, versionTwo, versionOne);

        _apiClient.Setup(x => x.Get<QualificationDiscussionHistoriesResponse>(
                It.IsAny<GetDiscussionHistoryForQualificationApiRequest>()))
            .ReturnsAsync(discussionHistory);

        _apiClient.Setup(x => x.Get<GetQualificationDetailsQueryResponse>(
                It.IsAny<GetQualificationDetailWithVersionsApiRequest>()))
            .ReturnsAsync(qualification);

        var result = await handler.Handle(
            new GetQualificationTimelineQuery { QualificationReference = QualificationReference },
            CancellationToken.None);

        var items = result.Value!.QualificationDiscussionHistories;

        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.Equal(3, items.Count);
            Assert.Equal(VersionThreeTimestamp, items[0].Timestamp);
            Assert.Equal(VersionTwoTimestamp, items[1].Timestamp);
            Assert.Equal(ExistingHistoryTimestamp, items[2].Timestamp);
        });
    }
}