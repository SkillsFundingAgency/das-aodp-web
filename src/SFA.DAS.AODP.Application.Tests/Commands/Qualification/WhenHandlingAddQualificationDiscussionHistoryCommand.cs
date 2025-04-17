using AutoFixture;
using Moq;
using SFA.DAS.AODP.Application.Commands.Qualification;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Tests.Commands.Qualification
{
    public class WhenHandlingAddQualificationDiscussionHistoryCommand
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly AddQualificationDiscussionHistoryCommandHandler _handler;


        public WhenHandlingAddQualificationDiscussionHistoryCommand()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            // Arrange
            var expectedResponse = _fixture.Create<EmptyResponse>();
            var request = _fixture.Create<AddQualificationDiscussionHistoryCommand>();
            _apiClient
                .Setup(a => a.PostWithResponseCode<EmptyResponse>(It.IsAny<AddQualificationDiscussionHistoryApiRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            _apiClient
                .Verify(a => a.PostWithResponseCode<EmptyResponse>(It.Is<AddQualificationDiscussionHistoryApiRequest>(r => r.Data == request)));

            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.NotNull(response.Value);
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var expectedException = _fixture.Create<Exception>();
            var request = _fixture.Create<AddQualificationDiscussionHistoryCommand>();
            _apiClient
                .Setup(a => a.PostWithResponseCode<EmptyResponse>(It.IsAny<AddQualificationDiscussionHistoryApiRequest>()))
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



