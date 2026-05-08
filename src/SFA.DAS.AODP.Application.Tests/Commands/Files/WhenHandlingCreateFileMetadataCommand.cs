using AutoFixture;
using Moq;
using SFA.DAS.AODP.Application.Commands.Files;
using SFA.DAS.AODP.Domain.Files;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Tests.Commands.Files
{
    public class WhenHandlingCreateFileMetadataCommand
    {
        private const string ExpectedErrorMessage = "api failed";

        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly CreateFileMetadataCommandHandler _handler;

        public WhenHandlingCreateFileMetadataCommand()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            // Arrange
            var request = _fixture.Create<CreateFileMetadataCommand>();

            _apiClient
                .Setup(a => a.PostWithResponseCode<EmptyResponse>(
                    It.IsAny<CreateFileMetadataApiRequest>()))
                .Returns(Task.FromResult(new EmptyResponse()));

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            Assert.Multiple(() =>
            {
                _apiClient.Verify(a =>
                    a.PostWithResponseCode<EmptyResponse>(
                        It.Is<CreateFileMetadataApiRequest>(r => r.Data == request)),
                    Times.Once);

                Assert.NotNull(response);
                Assert.True(response.Success);
                Assert.NotNull(response.Value);
            });
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var request = _fixture.Create<CreateFileMetadataCommand>();
            var expectedException = new Exception(ExpectedErrorMessage);

            _apiClient
                .Setup(a => a.PostWithResponseCode<EmptyResponse>(
                    It.IsAny<CreateFileMetadataApiRequest>()))
                .ThrowsAsync(expectedException);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            Assert.Multiple(() =>
            {
                _apiClient.Verify(a =>
                    a.PostWithResponseCode<EmptyResponse>(
                        It.Is<CreateFileMetadataApiRequest>(r => r.Data == request)),
                    Times.Once);

                Assert.NotNull(response);
                Assert.False(response.Success);
                Assert.Equal(ExpectedErrorMessage, response.ErrorMessage);
                Assert.NotNull(response.Value);
            });
        }
    }
}