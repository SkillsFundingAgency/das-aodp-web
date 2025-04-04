using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Domain.Interfaces;
using Xunit;
using System.Threading;
using System.Threading.Tasks;
using System;
using SFA.DAS.AODP.Application.Commands.Feedback;

namespace SFA.DAS.AODP.Application.Tests.Commands.Application
{
    public class SaveSurveyCommandHandlerTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IApiClient> _apiClientMock;
        private readonly SaveSurveyCommandHandler _handler;

        public SaveSurveyCommandHandlerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = new SaveSurveyCommandHandler(_apiClientMock.Object);
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            // Arrange
            var request = _fixture.Create<SaveSurveyCommand>();
            var expectedResponse = new BaseMediatrResponse<EmptyResponse> { Success = true };
            _apiClientMock
                .Setup(a => a.PostWithResponseCode<EmptyResponse>(It.IsAny<SaveSurveyApiRequest>()))
                .ReturnsAsync(expectedResponse.Value);

            // Act
            var response = await _handler.Handle(request, CancellationToken.None);

            // Assert
            _apiClientMock
                .Verify(a => a.PostWithResponseCode<EmptyResponse>(It.Is<SaveSurveyApiRequest>(r => r.Data == request)), Times.Once);

            Assert.NotNull(response);
            Assert.True(response.Success);
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var request = _fixture.Create<SaveSurveyCommand>();
            var expectedException = _fixture.Create<Exception>();
            _apiClientMock
                .Setup(a => a.PostWithResponseCode<EmptyResponse>(It.IsAny<SaveSurveyApiRequest>()))
                .ThrowsAsync(expectedException);

            // Act
            var response = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal(expectedException.Message, response.ErrorMessage);
        }
    }
}
