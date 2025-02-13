﻿using AutoFixture;
using Moq;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Tests.Commands.FormBuilder.Pages
{
    public class WhenHandlingDeletePageCommand
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly DeletePageCommandHandler _handler;


        public WhenHandlingDeletePageCommand()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            // Arrange
            var expectedResponse = _fixture
                .Build<BaseMediatrResponse<DeletePageCommandResponse>>()
                .With(w => w.Success, true)
                .Create();

            var request = _fixture.Create<DeletePageCommand>();

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            _apiClient
                .Verify(a => a.Delete(It.Is<DeletePageApiRequest>(r => r.PageId == request.PageId)));

            Assert.True(response.Success);
            Assert.NotNull(response.Value);
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var expectedException = _fixture.Create<Exception>();
            var request = _fixture.Create<DeletePageCommand>();
            _apiClient
                .Setup(a => a.Delete(It.IsAny<DeletePageApiRequest>()))
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
