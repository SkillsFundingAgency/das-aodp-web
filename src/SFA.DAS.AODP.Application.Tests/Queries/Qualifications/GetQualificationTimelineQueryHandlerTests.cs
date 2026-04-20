using Moq;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Application.Services;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.Qualifications;

public class GetQualificationTimelineQueryHandlerTests
{
    private const string QualificationReference = "61054902";
    private const string ExistingTitle = "Existing";
    private const string ExistingNote = "Existing note";
    private const string ExistingUser = "User";
    private const string ErrorPrefixDiscussionHistory = "Failed to get qualification discussion history";
    private const string ErrorPrefixVersions = "Failed to get qualification versions";
    private const string ExceptionMessage = "boom";

    private const int VersionOne = 1;
    private const int VersionTwo = 2;
    private const int VersionThree = 3;

    private static readonly DateTime ExistingHistoryTimestamp = new(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime VersionOneTimestamp = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime VersionTwoTimestamp = new(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime VersionThreeTimestamp = new(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IApiClient> _apiClient = new();
    private readonly Mock<IQualificationTimelineHistoryBuilder> _timelineBuilder = new();

    private GetQualificationTimelineQueryHandler CreateHandler()
    {
        return new GetQualificationTimelineQueryHandler(_apiClient.Object, _timelineBuilder.Object);
    }

    private async Task<BaseMediatrResponse<QualificationDiscussionHistoriesResponse>> Handle(
        QualificationDiscussionHistoriesResponse? discussionHistoryResponse,
        GetQualificationDetailsQueryResponse? qualificationResponse,
        List<QualificationDiscussionHistory>? generatedEntries = null)
    {
        var handler = CreateHandler();

        _apiClient.Setup(x => x.Get<QualificationDiscussionHistoriesResponse>(
                It.IsAny<GetDiscussionHistoryForQualificationApiRequest>()))
            .ReturnsAsync(discussionHistoryResponse!);

        _apiClient.Setup(x => x.Get<GetQualificationDetailsQueryResponse>(
                It.IsAny<GetQualificationDetailWithVersionsApiRequest>()))
            .ReturnsAsync(qualificationResponse!);

        _timelineBuilder.Setup(x => x.BuildTimelineEntries(It.IsAny<List<GetQualificationDetailsQueryResponse>>()))
            .Returns(generatedEntries ?? new List<QualificationDiscussionHistory>());

        return await handler.Handle(
            new GetQualificationTimelineQuery { QualificationReference = QualificationReference },
            CancellationToken.None);
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
                QualificationName = "Qualification",
                Versions = versions.ToList()
            }
        };
    }

    private static GetQualificationDetailsQueryResponse CreateVersion(int version, DateTime insertedTimestamp)
    {
        return new GetQualificationDetailsQueryResponse
        {
            Id = Guid.NewGuid(),
            QualificationId = Guid.NewGuid(),
            Version = version,
            InsertedTimestamp = insertedTimestamp,
            LastUpdatedDate = insertedTimestamp,
            Qual = new GetQualificationDetailsQueryResponse.Qualification
            {
                Id = Guid.NewGuid(),
                Qan = QualificationReference,
                QualificationName = "Qualification",
                Versions = new List<GetQualificationDetailsQueryResponse>()
            },
            Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation
            {
                Id = Guid.NewGuid(),
                NameOfqual = "Organisation"
            }
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
    public async Task Handle_ReturnsFailure_WhenDiscussionHistoryApiThrowsException()
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
    public async Task Handle_ReturnsFailure_WhenQualificationVersionsApiThrowsException()
    {
        var handler = CreateHandler();

        _apiClient.Setup(x => x.Get<QualificationDiscussionHistoriesResponse>(
                It.IsAny<GetDiscussionHistoryForQualificationApiRequest>()))
            .ReturnsAsync(CreateDiscussionHistoryResponse());

        _apiClient.Setup(x => x.Get<GetQualificationDetailsQueryResponse>(
                It.IsAny<GetQualificationDetailWithVersionsApiRequest>()))
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
    public async Task Handle_InitialisesDiscussionHistoryList_WhenNull()
    {
        var version = CreateVersion(VersionOne, VersionOneTimestamp);

        var result = await Handle(
            new QualificationDiscussionHistoriesResponse { QualificationDiscussionHistories = null! },
            CreateQualificationWithVersionsResponse(version));

        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.QualificationDiscussionHistories);
            Assert.Empty(result.Value.QualificationDiscussionHistories);
        });
    }

    [Fact]
    public async Task Handle_CallsTimelineBuilder_WithVersionsFromQualification()
    {
        var versionOne = CreateVersion(VersionOne, VersionOneTimestamp);
        var versionTwo = CreateVersion(VersionTwo, VersionTwoTimestamp);

        await Handle(
            CreateDiscussionHistoryResponse(),
            CreateQualificationWithVersionsResponse(versionOne, versionTwo));

        _timelineBuilder.Verify(x => x.BuildTimelineEntries(
            It.Is<List<GetQualificationDetailsQueryResponse>>(v =>
                v.Count == 2 &&
                v.Any(x => x.Version == VersionOne) &&
                v.Any(x => x.Version == VersionTwo))),
            Times.Once);
    }

    [Fact]
    public async Task Handle_PassesEmptyVersionsToTimelineBuilder_WhenQualIsNull()
    {
        var qualificationResponse = new GetQualificationDetailsQueryResponse
        {
            Qual = null
        };

        await Handle(
            CreateDiscussionHistoryResponse(),
            qualificationResponse);

        _timelineBuilder.Verify(x => x.BuildTimelineEntries(
            It.Is<List<GetQualificationDetailsQueryResponse>>(v => v.Count == 0)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_PassesEmptyVersionsToTimelineBuilder_WhenVersionsAreNull()
    {
        var qualificationResponse = new GetQualificationDetailsQueryResponse
        {
            Qual = new GetQualificationDetailsQueryResponse.Qualification
            {
                Id = Guid.NewGuid(),
                Qan = QualificationReference,
                QualificationName = "Qualification",
                Versions = null!
            }
        };

        await Handle(
            CreateDiscussionHistoryResponse(),
            qualificationResponse);

        _timelineBuilder.Verify(x => x.BuildTimelineEntries(
            It.Is<List<GetQualificationDetailsQueryResponse>>(v => v.Count == 0)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MergesExistingHistory_WithGeneratedEntries()
    {
        var existingHistory = new QualificationDiscussionHistory
        {
            Id = Guid.NewGuid(),
            QualificationId = Guid.NewGuid(),
            Title = ExistingTitle,
            Notes = ExistingNote,
            UserDisplayName = ExistingUser,
            Timestamp = ExistingHistoryTimestamp
        };

        var generatedEntry = new QualificationDiscussionHistory
        {
            Id = Guid.NewGuid(),
            QualificationId = Guid.NewGuid(),
            Title = "Generated",
            Notes = "Generated note",
            UserDisplayName = "Builder",
            Timestamp = VersionTwoTimestamp
        };

        var version = CreateVersion(VersionOne, VersionOneTimestamp);

        var result = await Handle(
            CreateDiscussionHistoryResponse(existingHistory),
            CreateQualificationWithVersionsResponse(version),
            new List<QualificationDiscussionHistory> { generatedEntry });

        var items = result.Value!.QualificationDiscussionHistories;

        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.Equal(2, items.Count);
            Assert.Contains(items, x => x.Title == ExistingTitle && x.Timestamp == ExistingHistoryTimestamp);
            Assert.Contains(items, x => x.Title == "Generated" && x.Timestamp == VersionTwoTimestamp);
        });
    }

    [Fact]
    public async Task Handle_ReturnsEntriesOrderedByTimestampDescending()
    {
        var existingHistory = new QualificationDiscussionHistory
        {
            Id = Guid.NewGuid(),
            QualificationId = Guid.NewGuid(),
            Title = ExistingTitle,
            Notes = ExistingNote,
            UserDisplayName = ExistingUser,
            Timestamp = ExistingHistoryTimestamp
        };

        var generatedEntries = new List<QualificationDiscussionHistory>
        {
            new QualificationDiscussionHistory
            {
                Title = "Generated newest",
                Timestamp = VersionThreeTimestamp
            },
            new QualificationDiscussionHistory
            {
                Title = "Generated middle",
                Timestamp = VersionTwoTimestamp
            }
        };

        var versionOne = CreateVersion(VersionOne, VersionOneTimestamp);
        var versionTwo = CreateVersion(VersionTwo, VersionTwoTimestamp);
        var versionThree = CreateVersion(VersionThree, VersionThreeTimestamp);

        var result = await Handle(
            CreateDiscussionHistoryResponse(existingHistory),
            CreateQualificationWithVersionsResponse(versionThree, versionTwo, versionOne),
            generatedEntries);

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