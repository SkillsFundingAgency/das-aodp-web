using AutoFixture;
using AutoFixture.AutoMoq;
using Azure;
using Moq;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Infrastructure.Cache;

namespace SFA.DAS.Aodp.Application.Tests.Queries.Application.Application
{
    public class WhenHandlingGetApplicationMetadataByIdQuery
    {
        private Mock<ICacheService> _cacheServiceMock;
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private AODP.Application.Queries.Application.Application.GetApplicationMetadataByIdQueryHandler _handler;


        public WhenHandlingGetApplicationMetadataByIdQuery()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _cacheServiceMock = _fixture.Freeze<Mock<ICacheService>>();
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<AODP.Application.Queries.Application.Application.GetApplicationMetadataByIdQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationMetadataByIdQuery>();

            var response = _fixture.Create<GetApplicationMetadataByIdQueryResponse>();

            _cacheServiceMock
              .Setup(x => x.GetAsync(
                  It.IsAny<string>(),
                  It.IsAny<Func<Task<GetApplicationMetadataByIdQueryResponse>>>()))
              .Callback<string, Func<Task<GetApplicationMetadataByIdQueryResponse>>>(async (key, func) =>
              {
                  await func();
              }).ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetApplicationMetadataByIdQueryResponse>(It.IsAny<GetApplicationMetadataByIdRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetApplicationMetadataByIdQueryResponse>(It.Is<GetApplicationMetadataByIdRequest>(r => r.ApplicationId == query.ApplicationId)), Times.Once);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Value.ApplicationId);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationMetadataByIdQuery>();
            var exception = _fixture.Create<Exception>();

            _cacheServiceMock
              .Setup(x => x.GetAsync(
                  It.IsAny<string>(),
                  It.IsAny<Func<Task<GetApplicationMetadataByIdQueryResponse>>>()))
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
