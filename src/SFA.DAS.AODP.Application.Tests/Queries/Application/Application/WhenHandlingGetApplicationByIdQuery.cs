using AutoFixture;
using AutoFixture.AutoMoq;
using Azure;
using Moq;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.Aodp.Application.Tests.Queries.Application.Application
{
    public class WhenHandlingGetApplicationByIdQuery
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private AODP.Application.Queries.Application.Application.GetApplicationByIdQueryHandler _handler;

        
        public WhenHandlingGetApplicationByIdQuery()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<AODP.Application.Queries.Application.Application.GetApplicationByIdQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationByIdQuery>();

            var response = _fixture.Create<GetApplicationByIdQueryResponse>();

            _apiClientMock.Setup(x => x.Get<GetApplicationByIdQueryResponse>(It.IsAny<GetApplicationByIdRequest>()))
                          .ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetApplicationByIdQueryResponse>(It.IsAny<GetApplicationByIdRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetApplicationByIdQueryResponse>(It.Is<GetApplicationByIdRequest>(r => r.ApplicationId == query.ApplicationId)), Times.Once);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Value.Name);
            Assert.Equal(response, result.Value);
        }

       [Fact]
       public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationByIdQuery>();
            var exception = _fixture.Create<Exception>();

            _apiClientMock.Setup(x => x.Get<GetApplicationByIdQueryResponse>(It.IsAny<GetApplicationByIdRequest>()))
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
