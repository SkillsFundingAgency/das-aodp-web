using AutoFixture;
using Moq;
using SFA.DAS.AODP.Application.Commands.Import;
using SFA.DAS.AODP.Domain.Import;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Tests.Commands.Import
{
    public class WhenHandlingRequestJobRunCommand
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly RequestJobRunCommandHandler _handler;


        public WhenHandlingRequestJobRunCommand()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            // Arrange
            var expectedResponse = _fixture.Create<EmptyResponse>();
            var request = _fixture.Create<RequestJobRunCommand>();
            _apiClient
                .Setup(a => a.PostWithResponseCode<EmptyResponse>(It.IsAny<RequestJobRunApiRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            _apiClient
                .Verify(a => a.PostWithResponseCode<EmptyResponse>(It.Is<RequestJobRunApiRequest>(r => r.Data == request)));

            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.NotNull(response.Value);
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var expectedException = _fixture.Create<Exception>();
            var request = _fixture.Create<RequestJobRunCommand>();
            _apiClient
                .Setup(a => a.PostWithResponseCode<EmptyResponse>(It.IsAny<RequestJobRunApiRequest>()))
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
}



