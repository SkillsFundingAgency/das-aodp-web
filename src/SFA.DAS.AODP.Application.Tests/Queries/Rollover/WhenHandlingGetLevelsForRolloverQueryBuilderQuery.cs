using Moq;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;
using SFA.DAS.AODP.Domain.ValueObjects;
using Shouldly;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.Rollover;

public class WhenHandlingGetLevelsForRolloverQueryBuilderQuery
{
    private readonly Mock<IApiClient> _apiClientMock = new();
    private readonly GetLevelsForRolloverQueryBuilderQueryHandler _handler;

    public WhenHandlingGetLevelsForRolloverQueryBuilderQuery()
    {
        _handler = new GetLevelsForRolloverQueryBuilderQueryHandler(_apiClientMock.Object);
    }

    [Fact]
    public async Task Handle_WhenApiReturnsLevels_ShouldMapKnownAndUnknownLevelIds()
    {
        // Arrange
        _apiClientMock
            .Setup(client => client.Get<GetLevelsForRolloverQueryBuilderQueryResponse>(
                It.IsAny<GetLevelsForRolloverQueryBuilderApiRequest>()))
            .ReturnsAsync(new GetLevelsForRolloverQueryBuilderQueryResponse
            {
                Levels = [QualificationLevel.Level3, new QualificationLevel(123, "API value")]
            });

        // Act
        var result = await _handler.Handle(new GetLevelsForRolloverQueryBuilderQuery(), CancellationToken.None);

        // Assert
        result.Success.ShouldBeTrue();
        result.Value.Levels.ShouldBe([QualificationLevel.Level3, QualificationLevel.Unspecified]);
    }

    [Fact]
    public async Task Handle_WhenApiThrows_ShouldReturnFailedResponse()
    {
        // Arrange
        _apiClientMock
            .Setup(client => client.Get<GetLevelsForRolloverQueryBuilderQueryResponse>(
                It.IsAny<GetLevelsForRolloverQueryBuilderApiRequest>()))
            .ThrowsAsync(new InvalidOperationException("API failed"));

        // Act
        var result = await _handler.Handle(new GetLevelsForRolloverQueryBuilderQuery(), CancellationToken.None);

        // Assert
        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("API failed");
    }
}
