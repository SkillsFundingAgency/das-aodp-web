using AutoFixture;
using Moq;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Tests.Commands.FormBuilder.Forms
{
    public class WhenHandlingCreateFormVersionCommand
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly CreateFormVersionCommandHandler _handler;


        public WhenHandlingCreateFormVersionCommand()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            // Arrange
            var expectedResponse = _fixture.Create<CreateFormVersionCommandResponse>();
            var request = _fixture.Create<CreateFormVersionCommand>();
            _apiClient
                .Setup(a => a.PostWithResponseCode<CreateFormVersionCommandResponse>(It.IsAny<CreateFormVersionApiRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            _apiClient
                .Verify(a => a.PostWithResponseCode<CreateFormVersionCommandResponse>(It.Is<CreateFormVersionApiRequest>(r => r.Data == request)));

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
            var request = _fixture.Create<CreateFormVersionCommand>();
            _apiClient
                .Setup(a => a.PostWithResponseCode<CreateFormVersionCommandResponse>(It.IsAny<CreateFormVersionApiRequest>()))
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