using AutoFixture;
using Moq;
using SFA.DAS.AODP.Application.Commands.Application.Application;
using SFA.DAS.AODP.Domain.Interfaces;
using Xunit;

namespace SFA.DAS.AODP.Application.Tests.Commands.Application.Application
{
    public class WhenHandlingWithdrawApplicationCommand
    {
        private const string ExceptionMessage = "Test exception message";

        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly WithdrawApplicationCommandHandler _handler;

        public WhenHandlingWithdrawApplicationCommand()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            var expectedResponse = _fixture.Create<EmptyResponse>();
            var request = _fixture.Create<WithdrawApplicationCommand>();

            _apiClient
                .Setup(a => a.PostWithResponseCode<EmptyResponse>(It.IsAny<WithdrawApplicationApiRequest>()))
                .ReturnsAsync(expectedResponse);

            var response = await _handler.Handle(request, default);

            Assert.Multiple(() =>
            {
                _apiClient.Verify(a =>
                    a.PostWithResponseCode<EmptyResponse>(
                        It.Is<WithdrawApplicationApiRequest>(r => r.Data == request)),
                    Times.Once);

                Assert.NotNull(response);
                Assert.True(response.Success);
                Assert.Null(response.ErrorMessage);
            });
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            var request = _fixture.Create<WithdrawApplicationCommand>();
            var expectedException = new Exception(ExceptionMessage);

            _apiClient
                .Setup(a => a.PostWithResponseCode<EmptyResponse>(It.IsAny<WithdrawApplicationApiRequest>()))
                .ThrowsAsync(expectedException);

            var response = await _handler.Handle(request, default);

            Assert.Multiple(() =>
            {
                Assert.NotNull(response);
                Assert.False(response.Success);
                Assert.NotNull(response.ErrorMessage);
                Assert.NotEmpty(response.ErrorMessage!);
                Assert.Equal(ExceptionMessage, response.ErrorMessage);
            });
        }
    }
}
