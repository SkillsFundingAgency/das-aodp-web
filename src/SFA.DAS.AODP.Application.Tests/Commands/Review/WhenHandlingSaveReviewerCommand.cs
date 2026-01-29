using AutoFixture;
using Moq;
using SFA.DAS.AODP.Domain.Interfaces;


namespace SFA.DAS.AODP.Application.Tests.Commands.Application.Application
{
    public class WhenHandlingSaveReviewerCommand
    {
        private const string ExceptionMessage = "Test exception message";

        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly SaveReviewerCommandHandler _handler;

        public WhenHandlingSaveReviewerCommand()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            var expectedResponse = _fixture.Create<SaveReviewerCommandResponse>();
            var request = _fixture.Create<SaveReviewerCommand>();

            _apiClient
                .Setup(a => a.Put<SaveReviewerCommandResponse>(It.IsAny<SaveReviewerApiRequest>()))
                .ReturnsAsync(expectedResponse);

            var response = await _handler.Handle(request, default);

            Assert.Multiple(() =>
            {
                _apiClient.Verify(a =>
                    a.Put<SaveReviewerCommandResponse>(
                        It.Is<SaveReviewerApiRequest>(r => r.Data == request)),
                    Times.Once);

                Assert.NotNull(response);
                Assert.True(response.Success);
                Assert.Null(response.ErrorMessage);
                Assert.NotNull(response.Value);
                Assert.Equal(expectedResponse, response.Value);
            });
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            var request = _fixture.Create<SaveReviewerCommand>();
            var expectedException = new Exception(ExceptionMessage);

            _apiClient
                .Setup(a => a.Put<SaveReviewerCommandResponse>(It.IsAny<SaveReviewerApiRequest>()))
                .ThrowsAsync(expectedException);

            var response = await _handler.Handle(request, default);

            Assert.Multiple(() =>
            {
                Assert.NotNull(response);
                Assert.False(response.Success);
                Assert.NotNull(response.ErrorMessage);
                Assert.NotEmpty(response.ErrorMessage!);
                Assert.Equal(ExceptionMessage, response.ErrorMessage);
                Assert.NotNull(response.Value);
            });
        }
    }
}
