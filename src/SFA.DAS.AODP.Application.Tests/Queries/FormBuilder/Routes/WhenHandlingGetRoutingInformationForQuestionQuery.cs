using AutoFixture;
using AutoFixture.AutoMoq;
using Azure;
using Moq;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Routes;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.Aodp.Application.Tests.Queries.FormBuilder.Routes
{
    public class WhenHandlingGetRoutingInformationForQuestionQuery
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private GetRoutingInformationForQuestionQueryHandler _handler;

        public WhenHandlingGetRoutingInformationForQuestionQuery()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<GetRoutingInformationForQuestionQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetRoutingInformationForQuestionQuery>();

            var response = _fixture.Create<GetRoutingInformationForQuestionQueryResponse>();

            _apiClientMock.Setup(x => x.Get<GetRoutingInformationForQuestionQueryResponse>(It.IsAny<GetRoutingInformationForQuestionApiRequest>()))
                          .ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetRoutingInformationForQuestionQueryResponse>(It.IsAny<GetRoutingInformationForQuestionApiRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetRoutingInformationForQuestionQueryResponse>(It.Is<GetRoutingInformationForQuestionApiRequest>(r => r.FormVersionId == query.FormVersionId)), Times.Once);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetRoutingInformationForQuestionQuery>();
            var exception = _fixture.Create<Exception>();

            _apiClientMock.Setup(x => x.Get<GetRoutingInformationForQuestionQueryResponse>(It.IsAny<GetRoutingInformationForQuestionApiRequest>()))
                          .ThrowsAsync(exception);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotEmpty(result.ErrorMessage);
            Assert.Equal(exception.Message, result.ErrorMessage);
        }
    }
}
