using AutoFixture;
using Moq;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Tests.Commands.FormBuilder.Pages
{
    public class WhenHandlingMovePageDownCommand
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly MovePageDownCommandHandler _handler;


        public WhenHandlingMovePageDownCommand()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            // Arrange
            var expectedResponse = _fixture.Create<MovePageDownCommandResponse>();
            var request = _fixture.Create<MovePageDownCommand>();
            _apiClient
                .Setup(a => a.Put(It.IsAny<MovePageDownApiRequest>()));

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            _apiClient
                .Verify(a => a.Put(It.Is<MovePageDownApiRequest>(r => r.PageId == request.PageId)));

            Assert.NotNull(response);
            Assert.True(response.Success);

            Assert.NotNull(response.Value);
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var expectedException = _fixture.Create<Exception>();
            var request = _fixture.Create<MovePageDownCommand>();
            _apiClient
                .Setup(a => a.Put(It.IsAny<MovePageDownApiRequest>()))
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