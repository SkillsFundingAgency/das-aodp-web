using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Application.Queries.Application.Form;
using SFA.DAS.AODP.Domain.Application.Form;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.Aodp.UnitTests.Application.Queries.Application.Form
{
    public class GetFormPreviewByIdQueryHandlerTests
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private GetFormPreviewByIdQueryHandler _handler;


        public GetFormPreviewByIdQueryHandlerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<GetFormPreviewByIdQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetFormPreviewByIdQuery>();

            var response = _fixture.Create<GetFormPreviewByIdQueryResponse>();

            _apiClientMock.Setup(x => x.Get<GetFormPreviewByIdQueryResponse>(It.IsAny<GetFormPreviewByIdApiRequest>()))
                          .ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetFormPreviewByIdQueryResponse>(It.IsAny<GetFormPreviewByIdApiRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetFormPreviewByIdQueryResponse>(It.Is<GetFormPreviewByIdApiRequest>(r => r._applicationId == query.ApplicationId)), Times.Once);

            Assert.True(result.Success);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetFormPreviewByIdQuery>();
            var exception = _fixture.Create<Exception>();

            _apiClientMock.Setup(x => x.Get<GetFormPreviewByIdQueryResponse>(It.IsAny<GetFormPreviewByIdApiRequest>()))
                          .ThrowsAsync(exception);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(exception.Message, result.ErrorMessage);
        }
    }
}
