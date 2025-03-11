using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Routes;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.Aodp.UnitTests.Application.Queries.FormBuilder.Routes
{
    public class GetAvailableSectionsAndPagesForRoutingHandlerTests
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private GetAvailableSectionsAndPagesForRoutingQueryHandler _handler;

        public GetAvailableSectionsAndPagesForRoutingHandlerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<GetAvailableSectionsAndPagesForRoutingQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetAvailableSectionsAndPagesForRoutingQuery>();

            var response = _fixture.Create<GetAvailableSectionsAndPagesForRoutingQueryResponse>();

            _apiClientMock.Setup(x => x.Get<GetAvailableSectionsAndPagesForRoutingQueryResponse>(It.IsAny<GetAvailableSectionsAndPagesForRoutingApiRequest>()))
                          .ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetAvailableSectionsAndPagesForRoutingQueryResponse>(It.IsAny<GetAvailableSectionsAndPagesForRoutingApiRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetAvailableSectionsAndPagesForRoutingQueryResponse>(It.Is<GetAvailableSectionsAndPagesForRoutingApiRequest>(r => r.FormVersionId == query.FormVersionId)), Times.Once);

            Assert.True(result.Success);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetAvailableSectionsAndPagesForRoutingQuery>();
            var exception = _fixture.Create<Exception>();

            _apiClientMock.Setup(x => x.Get<GetAvailableSectionsAndPagesForRoutingQueryResponse>(It.IsAny<GetAvailableSectionsAndPagesForRoutingApiRequest>()))
                          .ThrowsAsync(exception);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(exception.Message, result.ErrorMessage);
        }
    }
}
