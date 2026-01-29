using AutoFixture;
using Moq;
using SFA.DAS.AODP.Application.Commands.Application.Application;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Models;
using System.Net;

namespace SFA.DAS.AODP.Application.Tests.Commands.Application.Application
{
    public class WhenHandlingEditApplicationCommand
    {
        private const string ExpectedQanMessage = "all good";
        private const string ErrorContent = "";
        private const string ApplicationIdString = "11111111-1111-1111-1111-111111111111";
        private const string ExpectedErrorMessage = "api failed";
        private const bool ExpectedIsQanValid = true;

        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly EditApplicationCommandHandler _handler;

        public WhenHandlingEditApplicationCommand()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            var expectedBody = _fixture.Build<CreateApplicationCommandResponse>()
                .With(x => x.IsQanValid, ExpectedIsQanValid)
                .With(x => x.QanValidationMessage, ExpectedQanMessage)
                .Create();

            var apiResponse = new ApiResponse<CreateApplicationCommandResponse>(
                expectedBody,
                HttpStatusCode.OK,
                ErrorContent
            );

            var applicationId = Guid.Parse(ApplicationIdString);

            var request = _fixture.Build<EditApplicationCommand>()
                .With(x => x.ApplicationId, applicationId)
                .Create();

            _apiClient
                .Setup(a => a.PutWithResponseCode<CreateApplicationCommandResponse>(It.IsAny<EditApplicationApiRequest>()))
                .Returns(Task.FromResult(apiResponse));

            var response = await _handler.Handle(request, default);

            Assert.Multiple(() =>
            {
                _apiClient.Verify(a =>
                a.PutWithResponseCode<CreateApplicationCommandResponse>(
                    It.Is<EditApplicationApiRequest>(r =>
                        r.Data == request &&
                        r.ApplicationId == applicationId)
                ),
                Times.Once);

                Assert.NotNull(response);
                Assert.True(response.Success);
                Assert.NotNull(response.Value);
                Assert.Equal(ExpectedIsQanValid, response.Value.IsQanValid);
                Assert.Equal(ExpectedQanMessage, response.Value.QanValidationMessage);
            });
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            var expectedException = new Exception(ExpectedErrorMessage);

            var applicationId = Guid.Parse(ApplicationIdString);

            var request = _fixture.Build<EditApplicationCommand>()
                .With(x => x.ApplicationId, applicationId)
                .Create();

            _apiClient
                .Setup(a => a.PutWithResponseCode<CreateApplicationCommandResponse>(It.IsAny<EditApplicationApiRequest>()))
                .ThrowsAsync(expectedException);

            var response = await _handler.Handle(request, default);

            Assert.Multiple(() =>
            {
                _apiClient.Verify(a =>
                a.PutWithResponseCode<CreateApplicationCommandResponse>(
                    It.Is<EditApplicationApiRequest>(r =>
                        r.Data == request &&
                        r.ApplicationId == applicationId)
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
