using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Application.Commands.Review;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Models;
using System.Net;

namespace SFA.DAS.AODP.Application.UnitTests.Commands.Review;

public class BulkSaveReviewerCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IApiClient> _apiClientMock;
    private readonly BulkSaveReviewerCommandHandler _handler;

    public BulkSaveReviewerCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();

        _handler = new BulkSaveReviewerCommandHandler(_apiClientMock.Object);
    }

    [Fact]
    public async Task Handle_Returns_Success_Response_When_Api_Call_Succeeds()
    {
        var command = _fixture.Create<BulkSaveReviewerCommand>();

        var expectedBody = _fixture.Build<BulkSaveReviewerCommandResponse>()
                .Create();

        var apiResponse = new ApiResponse<BulkSaveReviewerCommandResponse>(
                expectedBody,
                HttpStatusCode.OK,
                string.Empty
            );

        _apiClientMock
            .Setup(x => x.PutWithResponseCode<BulkSaveReviewerCommandResponse>(
                It.Is<BulkSaveReviewerApiRequest>(r => r.Data == command)))
            .ReturnsAsync(apiResponse);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(apiResponse.Body, result.Value);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task Handle_Returns_Error_Response_When_Api_Call_Throws_Exception()
    {
        var command = _fixture.Create<BulkSaveReviewerCommand>();

        _apiClientMock
            .Setup(x => x.PutWithResponseCode<BulkSaveReviewerCommandResponse>(
                It.IsAny<BulkSaveReviewerApiRequest>()))
            .ThrowsAsync(new Exception("api failure"));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("api failure", result.ErrorMessage);
        Assert.NotNull(result.Value);
    }
}