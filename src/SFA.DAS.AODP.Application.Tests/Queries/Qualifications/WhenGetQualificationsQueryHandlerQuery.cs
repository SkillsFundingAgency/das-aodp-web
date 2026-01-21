using Moq;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.Qualifications;

public class WhenGetQualificationsQueryHandlerQuery
{
    [Fact]
    public async Task Handle_WhenRequestProvided_ReturnsDummyResponse()
    {
        // Arrange
        var apiClientMock = new Mock<IApiClient>(MockBehavior.Strict);
        var handler = new GetQualificationsQueryHandler(apiClientMock.Object);

        var request = new GetQualificationsQuery
        {
            Skip = 10,
            Take = 25,
            Name = "plumbing"
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);

        Assert.NotNull(result.Value);
        Assert.Equal(5, result.Value.TotalRecords);
        Assert.Equal(request.Skip, result.Value.Skip);
        Assert.Equal(request.Take, result.Value.Take);

        Assert.NotNull(result.Value.Data);
        Assert.Equal(5, result.Value.Data.Count);

        Assert.Equal("QAN-0001", result.Value.Data[0].Reference);
        Assert.Contains("Plumbing", result.Value.Data[0].Title, StringComparison.OrdinalIgnoreCase);

        apiClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenRequestIsNull_CatchesExceptionAndReturnsErrorResponse()
    {
        // Arrange
        var apiClientMock = new Mock<IApiClient>(MockBehavior.Strict);
        var handler = new GetQualificationsQueryHandler(apiClientMock.Object);

        // Act
        var result = await handler.Handle(null!, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));

        // Asert
        Assert.NotNull(result.Value);
        Assert.Equal(0, result.Value.TotalRecords);
        Assert.Equal(0, result.Value.Skip);
        Assert.Equal(0, result.Value.Take);
        Assert.True(result.Value.Data == null || result.Value.Data.Count == 0);

        apiClientMock.VerifyNoOtherCalls();
    }
}
