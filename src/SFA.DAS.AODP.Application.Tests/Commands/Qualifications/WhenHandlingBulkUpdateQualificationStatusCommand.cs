using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Application.Commands.Qualifications;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Models;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Tests.Commands.Qualifications;

public class WhenHandlingBulkUpdateQualificationStatusCommand
{
    private readonly IFixture _fixture;
    private readonly Mock<IApiClient> _apiClientMock;
    private readonly BulkUpdateQualificationStatusCommandHandler _handler;

    public WhenHandlingBulkUpdateQualificationStatusCommand()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
        _handler = new BulkUpdateQualificationStatusCommandHandler(_apiClientMock.Object);
    }

    [Fact]
    public async Task Then_ReturnsSuccess_AndPassesThroughApiResponseBody()
    {
        // Arrange
        var request = _fixture.Create<BulkUpdateQualificationStatusCommand>();
        var apiResponse = _fixture.Create<ApiResponse<BulkUpdateQualificationStatusCommandResponse>>();

        _apiClientMock
            .Setup(a => a.PutWithResponseCode<BulkUpdateQualificationStatusCommandResponse>(
                It.IsAny<BulkUpdateQualificationStatusApiRequest>()))
            .ReturnsAsync(apiResponse);

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.NotNull(response.Value);
        Assert.Same(apiResponse.Body, response.Value);
    }

    [Fact]
    public async Task And_ApiThrows_Then_ReturnsFailure_WithErrorMessage()
    {
        // Arrange
        var request = _fixture.Create<BulkUpdateQualificationStatusCommand>();
        var ex = _fixture.Create<Exception>();

        _apiClientMock
            .Setup(a => a.PutWithResponseCode<BulkUpdateQualificationStatusCommandResponse>(
                It.IsAny<BulkUpdateQualificationStatusApiRequest>()))
            .ThrowsAsync(ex);

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal(ex.Message, response.ErrorMessage);
    }
}