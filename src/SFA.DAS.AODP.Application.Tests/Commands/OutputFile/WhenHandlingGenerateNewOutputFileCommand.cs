using AutoFixture;
using Moq;
using SFA.DAS.AODP.Application.Commands.OutputFile;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.OutputFile;

namespace SFA.DAS.AODP.Application.UnitTests.Commands.OutputFile;

public class WhenHandlingGenerateNewOutputFileCommand
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IApiClient> _apiClient = new();
    private readonly GenerateNewOutputFileCommandHandler _handler;

    public WhenHandlingGenerateNewOutputFileCommand()
    {
        _handler = new(_apiClient.Object);
    }

    [Fact]
    public async Task Then_The_CommandResult_Is_Returned_As_Expected()
    {
        // Arrange
        var expectedResponse = _fixture
            .Build<BaseMediatrResponse<EmptyResponse>>()
            .With(w => w.Success, true)
            .Create();

        var request = _fixture.Create<GenerateNewOutputFileCommand>();

        // Act
        var response = await _handler.Handle(request, default);

        // Assert
        _apiClient
            .Verify(a => a.PostWithResponseCode<EmptyResponse>(It.IsAny<GenerateNewOutputFileApiRequest>()));

        Assert.True(response.Success);
        Assert.NotNull(response.Value);
    }

    [Fact]
    public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
    {
        // Arrange
        var expectedException = _fixture.Create<Exception>();
        var request = _fixture.Create<GenerateNewOutputFileCommand>();
        _apiClient
            .Setup(a => a.PostWithResponseCode<EmptyResponse>(It.IsAny<GenerateNewOutputFileApiRequest>()))
            .ThrowsAsync(expectedException);

        // Act
        var response = await _handler.Handle(request, default);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.NotEmpty(response.ErrorMessage!);
        Assert.Equal(expectedException.Message, response.ErrorMessage);
    }
}
