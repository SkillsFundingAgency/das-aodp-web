using AutoFixture;
using AutoFixture.AutoMoq;
using Azure;
using Moq;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Routes;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.Aodp.Application.Tests.Queries.FormBuilder.Routes
{
    public class WhenHandlingGetRoutingInformationForFormQuery
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private GetRoutingInformationForFormQueryHandler _handler;

        public WhenHandlingGetRoutingInformationForFormQuery()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<GetRoutingInformationForFormQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetRoutingInformationForFormQuery>();

            var response = new GetRoutingInformationForFormQueryResponse()
            {
                Sections = { },
                Editable = true
            };

            _apiClientMock.Setup(x => x.Get<GetRoutingInformationForFormQueryResponse>(It.IsAny<GetRoutesForFormVersionApiRequest>()))
                          .ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetRoutingInformationForFormQueryResponse>(It.IsAny<GetRoutesForFormVersionApiRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetRoutingInformationForFormQueryResponse>(It.Is<GetRoutesForFormVersionApiRequest>(r => r.FormVersionId == query.FormVersionId)), Times.Once);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.Equal(response.Editable, result.Value.Editable);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetRoutingInformationForFormQuery>();
            var exception = _fixture.Create<Exception>();

            _apiClientMock.Setup(x => x.Get<GetRoutingInformationForFormQueryResponse>(It.IsAny<GetRoutesForFormVersionApiRequest>()))
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
