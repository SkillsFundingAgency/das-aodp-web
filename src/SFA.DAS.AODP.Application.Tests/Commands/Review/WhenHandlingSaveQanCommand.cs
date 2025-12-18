using AutoFixture;
using Moq;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Models;
using System.Net;

namespace SFA.DAS.AODP.Application.Tests.Commands.Review
{
    public class WhenHandlingSaveQanCommand
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly SaveQanCommandHandler _handler;


        public WhenHandlingSaveQanCommand()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            // Arrange
            var request = _fixture.Create<SaveQanCommand>();

            var expectedBody = _fixture.Build<SaveQanCommandResponse>()
                .With(x => x.IsQanValid, true)
                .With(x => x.QanValidationMessage, "OK")
                .Create();

            var apiResponse = new ApiResponse<SaveQanCommandResponse>(
                expectedBody,
                HttpStatusCode.OK,
                string.Empty
            );

            _apiClient
                .Setup(a => a.PutWithResponseCode<SaveQanCommandResponse>(
                    It.IsAny<SaveQanApiRequest>()))
                .ReturnsAsync(apiResponse);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            _apiClient.Verify(a => a.PutWithResponseCode<SaveQanCommandResponse>(
                It.IsAny<SaveQanApiRequest>()),
                Times.Once);

            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.NotNull(response.Value);
            Assert.Equal(expectedBody.IsQanValid, response.Value.IsQanValid);
            Assert.Equal(expectedBody.QanValidationMessage, response.Value.QanValidationMessage);
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var request = _fixture.Create<SaveQanCommand>();
            var expectedException = new Exception("Exception");

            _apiClient
                .Setup(a => a.PutWithResponseCode<SaveQanCommandResponse>(It.IsAny<SaveQanApiRequest>()))
                .ThrowsAsync(expectedException);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            _apiClient.Verify(a => a.PutWithResponseCode<SaveQanCommandResponse>(It.IsAny<SaveQanApiRequest>()), Times.Once);

            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal(expectedException.Message, response.ErrorMessage);
        }
    }
}