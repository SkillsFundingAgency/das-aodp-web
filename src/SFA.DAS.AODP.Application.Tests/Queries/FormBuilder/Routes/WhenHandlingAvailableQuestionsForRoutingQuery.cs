using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Routes;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.Aodp.Application.Tests.Queries.FormBuilder.Routes
{
    public class WhenHandlingAvailableQuestionsForRoutingQuery
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private GetAvailableQuestionsForRoutingQueryHandler _handler;

        public WhenHandlingAvailableQuestionsForRoutingQuery()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<GetAvailableQuestionsForRoutingQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetAvailableQuestionsForRoutingQuery>();

            var response = _fixture.Create<GetAvailableQuestionsForRoutingQueryResponse>();

            _apiClientMock.Setup(x => x.Get<GetAvailableQuestionsForRoutingQueryResponse>(It.IsAny<GetAvailableQuestionsForRoutingApiRequest>()))
                          .ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetAvailableQuestionsForRoutingQueryResponse>(It.IsAny<GetAvailableQuestionsForRoutingApiRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetAvailableQuestionsForRoutingQueryResponse>(It.Is<GetAvailableQuestionsForRoutingApiRequest>(r => r.FormVersionId == query.FormVersionId)), Times.Once);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetAvailableQuestionsForRoutingQuery>();
            var exception = _fixture.Create<Exception>();

            _apiClientMock.Setup(x => x.Get<GetAvailableQuestionsForRoutingQueryResponse>(It.IsAny<GetAvailableQuestionsForRoutingApiRequest>()))
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
