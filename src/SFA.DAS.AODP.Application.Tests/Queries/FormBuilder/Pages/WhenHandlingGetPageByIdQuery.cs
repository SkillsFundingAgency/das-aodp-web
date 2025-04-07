using AutoFixture;
using AutoFixture.AutoMoq;
using Azure;
using Moq;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.Aodp.Application.Tests.Queries.FormBuilder.Pages
{
    public class WhenHandlingGetPageByIdQuery
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private GetPageByIdQueryHandler _handler;


        public WhenHandlingGetPageByIdQuery()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<GetPageByIdQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetPageByIdQuery>();

            var response = _fixture.Create<GetPageByIdQueryResponse>();

            _apiClientMock.Setup(x => x.Get<GetPageByIdQueryResponse>(It.IsAny<GetPageByIdApiRequest>()))
                          .ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetPageByIdQueryResponse>(It.IsAny<GetPageByIdApiRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetPageByIdQueryResponse>(It.Is<GetPageByIdApiRequest>(r => r.FormVersionId == query.FormVersionId)), Times.Once);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Value.Title);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            //var query = _fixture.Create<GetPageByIdQuery>();
            var query = new GetPageByIdQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            var exception = _fixture.Create<Exception>();

            _apiClientMock.Setup(x => x.Get<GetPageByIdQueryResponse>(It.IsAny<GetPageByIdApiRequest>()))
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
