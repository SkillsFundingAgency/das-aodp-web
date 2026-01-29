using AutoFixture;
using Moq;
using SFA.DAS.AODP.Application.Commands.Application.Application;
using SFA.DAS.AODP.Domain.Application.Application;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Tests.Commands.Application.Application
{
    public class WhenHandlingCreateApplicationCommand
    {
        private const string ExpectedQanMessage = "all good";
        private const string ExpectedErrorMessage = "api failed";
        private static readonly Guid ExpectedId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private const bool ExpectedIsQanValid = true;

        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly CreateApplicationCommandHandler _handler;

        public WhenHandlingCreateApplicationCommand()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            var expectedResponse = _fixture.Build<CreateApplicationCommandResponse>()
                .With(x => x.Id, ExpectedId)
                .With(x => x.IsQanValid, ExpectedIsQanValid)
                .With(x => x.QanValidationMessage, ExpectedQanMessage)
                .Create();

            var request = _fixture.Create<CreateApplicationCommand>();

            _apiClient
                .Setup(a => a.PostWithResponseCode<CreateApplicationCommandResponse>(It.IsAny<CreateApplicationApiRequest>()))
                .Returns(Task.FromResult(expectedResponse));

            var response = await _handler.Handle(request, default);

            Assert.Multiple(() =>
            {

                _apiClient.Verify(a =>
                    a.PostWithResponseCode<CreateApplicationCommandResponse>(
                        It.Is<CreateApplicationApiRequest>(r => r.Data == request)
                    ),
                    Times.Once);

                Assert.NotNull(response);
                Assert.True(response.Success);
                Assert.NotNull(response.Value);
                Assert.Equal(ExpectedId, response.Value.Id);
                Assert.Equal(ExpectedIsQanValid, response.Value.IsQanValid);
                Assert.Equal(ExpectedQanMessage, response.Value.QanValidationMessage);
            });
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            var expectedException = new Exception(ExpectedErrorMessage);
            var request = _fixture.Create<CreateApplicationCommand>();

            _apiClient
                .Setup(a => a.PostWithResponseCode<CreateApplicationCommandResponse>(It.IsAny<CreateApplicationApiRequest>()))
                .ThrowsAsync(expectedException);

            var response = await _handler.Handle(request, default);

            Assert.Multiple(() =>
            {
                _apiClient.Verify(a =>
                a.PostWithResponseCode<CreateApplicationCommandResponse>(
                    It.Is<CreateApplicationApiRequest>(r => r.Data == request)
                ),
                Times.Once);

                Assert.NotNull(response);
                Assert.False(response.Success);
                Assert.NotEmpty(response.ErrorMessage!);
                Assert.Equal(ExpectedErrorMessage, response.ErrorMessage);
                Assert.NotNull(response.Value);
            });
        }
    }
}
