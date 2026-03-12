using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Application.Commands.Review;
using SFA.DAS.AODP.Domain.Application.Application;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Models;
using System.Net;

namespace SFA.DAS.AODP.Application.UnitTests.Commands.Review;

public class BulkApplicationActionCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IApiClient> _apiClientMock;
    private readonly BulkApplicationActionCommandHandler _handler;

    public BulkApplicationActionCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();

        _handler = new BulkApplicationActionCommandHandler(_apiClientMock.Object);
    }

    [Fact]
    public async Task Handle_Returns_Success_Response_When_Api_Call_Succeeds()
    {
        var command = _fixture.Create<BulkApplicationActionCommand>();

        var expectedBody = _fixture.Build<BulkApplicationActionCommandResponse>()
            .Create();

        var apiResponse = new ApiResponse<BulkApplicationActionCommandResponse>(
            expectedBody,
            HttpStatusCode.OK,
            string.Empty
        );

        _apiClientMock
            .Setup(x => x.PutWithResponseCode<BulkApplicationActionCommandResponse>(
                It.Is<BulkApplicationActionApiRequest>(r => r.Data == command)))
            .ReturnsAsync(apiResponse);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(expectedBody, result.Value);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task Handle_Returns_Error_Response_When_Api_Call_Throws_Exception()
    {
        var command = _fixture.Create<BulkApplicationActionCommand>();

        _apiClientMock
            .Setup(x => x.PutWithResponseCode<BulkApplicationActionCommandResponse>(
                It.IsAny<BulkApplicationActionApiRequest>()))
            .ThrowsAsync(new Exception("api failure"));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("api failure", result.ErrorMessage);
        Assert.NotNull(result.Value);
    }

    private class TestApiResponse<T>
    {
        public T Body { get; set; } = default!;
    }
}