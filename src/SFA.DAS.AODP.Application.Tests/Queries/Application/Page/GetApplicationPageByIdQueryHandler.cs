using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Application.Queries.Application.Page;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Infrastructure.Cache;

namespace SFA.DAS.Aodp.UnitTests.Application.Queries.Application.Page
{
    public class GetApplicationPageByIdQueryHandlerTests
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private GetApplicationPageByIdQueryHandler _handler;
        private Mock<ICacheService> _cacheServiceMock;


        public GetApplicationPageByIdQueryHandlerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _cacheServiceMock = _fixture.Freeze<Mock<ICacheService>>();
            _handler = _fixture.Create<GetApplicationPageByIdQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationPageByIdQuery>();

            var response = new GetApplicationPageByIdQueryResponse()
            {
                Id = Guid.NewGuid(),
                Title = "test",
                Description = "test",
                Order = 1,
                TotalSectionPages = 1,
                Questions = {},
            };

            _apiClientMock.Setup(x => x.Get<GetApplicationPageByIdQueryResponse>(It.IsAny<GetApplicationPageByIdApiRequest>()))
                          .ReturnsAsync(response);

            _cacheServiceMock
               .Setup(x => x.GetAsync<GetApplicationPageByIdQueryResponse>(
                   It.IsAny<string>(),
                   It.IsAny<Func<Task<GetApplicationPageByIdQueryResponse>>>()))
               .Callback<string, Func<Task<GetApplicationPageByIdQueryResponse>>>(async (key, func) =>
               {
                   await func();
               }).ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetApplicationPageByIdQueryResponse>(It.IsAny<GetApplicationPageByIdApiRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetApplicationPageByIdQueryResponse>(It.Is<GetApplicationPageByIdApiRequest>(r => r.FormVersionId == query.FormVersionId)), Times.Once);

            Assert.True(result.Success);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationPageByIdQuery>();
            var exception = _fixture.Create<Exception>();                   

            _cacheServiceMock
              .Setup(x => x.GetAsync<GetApplicationPageByIdQueryResponse>(
                  It.IsAny<string>(),
                  It.IsAny<Func<Task<GetApplicationPageByIdQueryResponse>>>())).ThrowsAsync(exception);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(exception.Message, result.ErrorMessage);
        }
    }
}
