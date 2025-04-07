using AutoFixture;
using AutoFixture.AutoMoq;
using Azure;
using Moq;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Infrastructure.Cache;
using System;

namespace SFA.DAS.Aodp.Application.Tests.Queries.Application.Form
{
    public class WhenHandlingGetApplicationFormByIdQueryHandlerTests
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private Mock<ICacheService>? _cacheServiceMock;
        private AODP.Application.Queries.Application.Form.GetApplicationFormByIdQueryHandler _handler;


        public WhenHandlingGetApplicationFormByIdQueryHandlerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _cacheServiceMock = _fixture.Freeze<Mock<ICacheService>>();
            _handler = _fixture.Create<AODP.Application.Queries.Application.Form.GetApplicationFormByIdQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationFormByIdQuery>();

            var response = _fixture.Create<GetApplicationFormByIdQueryResponse>();

            _cacheServiceMock
                .Setup(x => x.GetAsync<GetApplicationFormByIdQueryResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<GetApplicationFormByIdQueryResponse>>>()))
                .Callback<string, Func<Task<GetApplicationFormByIdQueryResponse>>>(async (key, func) =>
                {
                    await func();
                }).ReturnsAsync(response);

            _apiClientMock.Setup(x => x.Get<GetApplicationFormByIdQueryResponse>(It.IsAny<GetApplicationFormByIdApiRequest>()))
                          .Returns(Task.FromResult(response));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetApplicationFormByIdQueryResponse>(It.IsAny<GetApplicationFormByIdApiRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetApplicationFormByIdQueryResponse>(It.Is<GetApplicationFormByIdApiRequest>(r => r.FormVersionId == query.FormVersionId)), Times.Once);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Value.FormTitle);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationFormByIdQuery>();
            var exception = _fixture.Create<Exception>();

            _cacheServiceMock
                .Setup(x => x.GetAsync<GetApplicationFormByIdQueryResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<GetApplicationFormByIdQueryResponse>>>()))
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
