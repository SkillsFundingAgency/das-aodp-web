using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.Aodp.UnitTests.Application.Queries.Application.Page
{
    public class GetApplicationPageAnswersByPageIdQueryHandlerTests
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private AODP.Application.Queries.Application.Page.GetApplicationPageAnswersByPageIdQueryHandler _handler;


        public GetApplicationPageAnswersByPageIdQueryHandlerTests()
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

            Assert.True(result.Success);
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
            Assert.False(result.Success);
            Assert.Equal(exception.Message, result.ErrorMessage);
        }
    }
}
