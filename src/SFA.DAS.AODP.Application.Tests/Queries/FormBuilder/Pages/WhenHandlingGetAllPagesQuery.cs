using AutoFixture;
using AutoFixture.AutoMoq;
using Azure;
using Moq;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.Aodp.Application.Tests.Queries.FormBuilder.Pages
{
    public class WhenHandlingGetAllPagesQuery
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private GetAllPagesQueryHandler _handler;


        public WhenHandlingGetAllPagesQuery()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<GetAllPagesQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetAllPagesQuery>();

            var response = _fixture.Create<GetAllPagesQueryResponse>();

            _apiClientMock.Setup(x => x.Get<GetAllPagesQueryResponse>(It.IsAny<GetAllPagesApiRequest>()))
                          .ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetAllPagesQueryResponse>(It.IsAny<GetAllPagesApiRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetAllPagesQueryResponse>(It.Is<GetAllPagesApiRequest>(r => r.FormVersionId == query.FormVersionId)), Times.Once);

            Assert.NotNull(response);
            Assert.True(result.Success);
            Assert.NotNull(response.Data.Count);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetAllPagesQuery>();
            var exception = _fixture.Create<Exception>();

            _apiClientMock.Setup(x => x.Get<GetAllPagesQueryResponse>(It.IsAny<GetAllPagesApiRequest>()))
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
