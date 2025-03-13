using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Infrastructure.Cache;

namespace SFA.DAS.Aodp.UnitTests.Application.Queries.Application.Section
{
    public class GetApplicationSectionByIdQueryHandlerTests
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private AODP.Application.Queries.Application.Section.GetApplicationSectionByIdQueryHandler _handler;
        private Mock<ICacheService> _cacheServiceMock;

        public GetApplicationSectionByIdQueryHandlerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _cacheServiceMock = _fixture.Freeze<Mock<ICacheService>>();
            _handler = _fixture.Create<AODP.Application.Queries.Application.Section.GetApplicationSectionByIdQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationSectionByIdQuery>();

            var response = _fixture.Create<GetApplicationSectionByIdQueryResponse>();

            _apiClientMock.Setup(x => x.Get<GetApplicationSectionByIdQueryResponse>(It.IsAny<GetApplicationSectionByIdApiRequest>()))
                          .ReturnsAsync(response);

            _cacheServiceMock
                .Setup(x => x.GetAsync<GetApplicationSectionByIdQueryResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<GetApplicationSectionByIdQueryResponse>>>()))
                .Callback<string, Func<Task<GetApplicationSectionByIdQueryResponse>>>(async (key, func) =>
                {
                    await func();
                }).ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetApplicationSectionByIdQueryResponse>(It.IsAny<GetApplicationSectionByIdApiRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetApplicationSectionByIdQueryResponse>(It.Is<GetApplicationSectionByIdApiRequest>(r => r.FormVersionId == query.FormVersionId)), Times.Once);

            Assert.True(result.Success);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationSectionByIdQuery>();
            var exception = _fixture.Create<Exception>();

            _cacheServiceMock
                .Setup(x => x.GetAsync<GetApplicationSectionByIdQueryResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<GetApplicationSectionByIdQueryResponse>>>()))
                          .ThrowsAsync(exception);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(exception.Message, result.ErrorMessage);
        }
    }
}
