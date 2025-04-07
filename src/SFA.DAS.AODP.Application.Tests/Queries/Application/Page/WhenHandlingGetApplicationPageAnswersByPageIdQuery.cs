using AutoFixture;
using AutoFixture.AutoMoq;
using Azure;
using Moq;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.Aodp.Application.Tests.Queries.Application.Page
{
    public class WhenHandlingGetApplicationPageAnswersByPageIdQuery
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private AODP.Application.Queries.Application.Page.GetApplicationPageAnswersByPageIdQueryHandler _handler;


        public WhenHandlingGetApplicationPageAnswersByPageIdQuery()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<AODP.Application.Queries.Application.Page.GetApplicationPageAnswersByPageIdQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationPageAnswersByPageIdQuery>();

            //var response = _fixture.Create<GetApplicationPageAnswersByPageIdQueryResponse>();

            var response = new GetApplicationPageAnswersByPageIdQueryResponse()
            {
                Questions = { }
            };

            _apiClientMock.Setup(x => x.Get<GetApplicationPageAnswersByPageIdQueryResponse>(It.IsAny<GetApplicationPageAnswersByPageIdApiRequest>()))
                          .ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetApplicationPageAnswersByPageIdQueryResponse>(It.IsAny<GetApplicationPageAnswersByPageIdApiRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetApplicationPageAnswersByPageIdQueryResponse>(It.Is<GetApplicationPageAnswersByPageIdApiRequest>(r => r.PageId == query.PageId)), Times.Once);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Value.Questions.Count);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationPageAnswersByPageIdQuery>();
            var exception = _fixture.Create<Exception>();

            _apiClientMock.Setup(x => x.Get<GetApplicationPageAnswersByPageIdQueryResponse>(It.IsAny<GetApplicationPageAnswersByPageIdApiRequest>()))
                          .ThrowsAsync(exception);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotEmpty(result.ErrorMessage!);
            Assert.Equal(exception.Message, result.ErrorMessage);
        }
    }
}
