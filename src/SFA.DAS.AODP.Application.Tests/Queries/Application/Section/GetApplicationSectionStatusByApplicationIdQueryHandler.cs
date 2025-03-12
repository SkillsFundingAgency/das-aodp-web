using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.Aodp.UnitTests.Application.Queries.Application.Section
{
    public class GetApplicationSectionStatusByApplicationIdQueryHandler
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private AODP.Application.Queries.Application.Section.GetApplicationSectionStatusByApplicationIdQueryHandler _handler;


        public GetApplicationSectionStatusByApplicationIdQueryHandler()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<AODP.Application.Queries.Application.Section.GetApplicationSectionStatusByApplicationIdQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationSectionStatusByApplicationIdQuery>();

            var response = _fixture.Create<GetApplicationSectionStatusByApplicationIdQueryResponse>();

            _apiClientMock.Setup(x => x.Get<GetApplicationSectionStatusByApplicationIdQueryResponse>(It.IsAny<GetApplicationSectionStatusByIdApiRequest>()))
                          .ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetApplicationSectionStatusByApplicationIdQueryResponse>(It.IsAny<GetApplicationSectionStatusByIdApiRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetApplicationSectionStatusByApplicationIdQueryResponse>(It.Is<GetApplicationSectionStatusByIdApiRequest>(r => r.FormVersionId == query.FormVersionId)), Times.Once);

            Assert.True(result.Success);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationSectionStatusByApplicationIdQuery>();
            var exception = _fixture.Create<Exception>();

            _apiClientMock.Setup(x => x.Get<GetApplicationSectionStatusByApplicationIdQueryResponse>(It.IsAny<GetApplicationSectionStatusByIdApiRequest>()))
                          .ThrowsAsync(exception);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(exception.Message, result.ErrorMessage);
        }
    }
}
