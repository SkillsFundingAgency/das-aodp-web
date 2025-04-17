using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Domain.Application.Application;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.Aodp.UnitTests.Application.Queries.Application.Application
{
    public class GetApplicationMessageByIdQueryHandlerTests
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private GetApplicationMessageByIdQueryHandler _handler;


        public GetApplicationMessageByIdQueryHandlerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<GetApplicationMessageByIdQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Message_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationMessageByIdQuery>();

            var response = _fixture.Create<GetApplicationMessageByIdQueryResponse>();

            _apiClientMock.Setup(x => x.Get<GetApplicationMessageByIdQueryResponse>(It.IsAny<GetApplicationMessageByIdApiRequest>()))
                          .ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetApplicationMessageByIdQueryResponse>(It.IsAny<GetApplicationMessageByIdApiRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetApplicationMessageByIdQueryResponse>(It.Is<GetApplicationMessageByIdApiRequest>(r => r.MessageId == query.MessageId)), Times.Once);

            Assert.True(result.Success);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationMessageByIdQuery>();
            var exception = _fixture.Create<Exception>();

            _apiClientMock.Setup(x => x.Get<GetApplicationMessageByIdQueryResponse>(It.IsAny<GetApplicationMessageByIdApiRequest>()))
                          .ThrowsAsync(exception);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(exception.Message, result.ErrorMessage);
        }
    }
}
