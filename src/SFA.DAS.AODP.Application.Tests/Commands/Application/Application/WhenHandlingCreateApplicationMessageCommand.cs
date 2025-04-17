using AutoFixture;
using Moq;
using SFA.DAS.AODP.Application.Commands.Application.Application;
using SFA.DAS.AODP.Domain.Application.Application;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Tests.Commands.Application.Application
{
    public class WhenHandlingCreateApplicationMessageCommandHandler
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly CreateApplicationMessageCommandHandler _handler;


        public WhenHandlingCreateApplicationMessageCommandHandler()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            // Arrange
            var expectedResponse = _fixture.Create<CreateApplicationMessageCommandResponse>();
            var request = _fixture.Create<CreateApplicationMessageCommand>();
            _apiClient
                .Setup(a => a.PostWithResponseCode<CreateApplicationMessageCommandResponse>(It.IsAny<CreateApplicationMessageApiRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            _apiClient
                .Verify(a => a.PostWithResponseCode<CreateApplicationMessageCommandResponse>(It.Is<CreateApplicationMessageApiRequest>(r => r.Data == request)));

            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.NotNull(response.Value);
            Assert.Equal(expectedResponse.Id, response.Value.Id);
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var expectedException = _fixture.Create<Exception>();
            var request = _fixture.Create<CreateApplicationMessageCommand>();
            _apiClient
                .Setup(a => a.PostWithResponseCode<CreateApplicationMessageCommandResponse>(It.IsAny<CreateApplicationMessageApiRequest>()))
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