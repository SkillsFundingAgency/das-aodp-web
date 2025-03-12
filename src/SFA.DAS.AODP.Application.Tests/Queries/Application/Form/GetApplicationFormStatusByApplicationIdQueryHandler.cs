using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Application.Queries.Application.Form;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.Aodp.UnitTests.Application.Queries.Application.Form
{
    public class GetApplicationFormStatusByApplicationIdQueryHandlerTests
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private GetApplicationFormStatusByApplicationIdQueryHandler _handler;


        public GetApplicationFormStatusByApplicationIdQueryHandlerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<GetApplicationFormStatusByApplicationIdQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationFormStatusByApplicationIdQuery>();

            var response = _fixture.Create<GetApplicationFormStatusByApplicationIdQueryResponse>();

            _apiClientMock.Setup(x => x.Get<GetApplicationFormStatusByApplicationIdQueryResponse>(It.IsAny<GetApplicationFormStatusByApplicationIdApiRequest>()))
                          .ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetApplicationFormStatusByApplicationIdQueryResponse>(It.IsAny<GetApplicationFormStatusByApplicationIdApiRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetApplicationFormStatusByApplicationIdQueryResponse>(It.Is<GetApplicationFormStatusByApplicationIdApiRequest>(r => r.FormVersionId == query.FormVersionId)), Times.Once);

            Assert.True(result.Success);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationFormStatusByApplicationIdQuery>();
            var exception = _fixture.Create<Exception>();

            _apiClientMock.Setup(x => x.Get<GetApplicationFormStatusByApplicationIdQueryResponse>(It.IsAny<GetApplicationFormStatusByApplicationIdApiRequest>()))
                          .ThrowsAsync(exception);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(exception.Message, result.ErrorMessage);
        }
    }
}
