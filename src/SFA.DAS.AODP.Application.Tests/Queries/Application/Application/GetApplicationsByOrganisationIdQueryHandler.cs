using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.Aodp.UnitTests.Application.Queries.Application.Application
{
    public class GetApplicationsByOrganisationIdQueryHandlerTests
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private AODP.Application.Queries.Application.Application.GetApplicationsByOrganisationIdQueryHandler _handler;


        public GetApplicationsByOrganisationIdQueryHandlerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<AODP.Application.Queries.Application.Application.GetApplicationsByOrganisationIdQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationsByOrganisationIdQuery>();

            var response = _fixture.Create<GetApplicationsByOrganisationIdQueryResponse>();

            _apiClientMock.Setup(x => x.Get<GetApplicationsByOrganisationIdQueryResponse>(It.IsAny<GetApplicationsByOrganisationIdApiRequest>()))
                          .ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetApplicationsByOrganisationIdQueryResponse>(It.IsAny<GetApplicationsByOrganisationIdApiRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetApplicationsByOrganisationIdQueryResponse>(It.Is<GetApplicationsByOrganisationIdApiRequest>(r => r.OrganisationId == query.OrganisationId)), Times.Once);

            Assert.True(result.Success);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationsByOrganisationIdQuery>();
            var exception = _fixture.Create<Exception>();

            _apiClientMock.Setup(x => x.Get<GetApplicationsByOrganisationIdQueryResponse>(It.IsAny<GetApplicationsByOrganisationIdApiRequest>()))
                          .ThrowsAsync(exception);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(exception.Message, result.ErrorMessage);
        }
    }
}
