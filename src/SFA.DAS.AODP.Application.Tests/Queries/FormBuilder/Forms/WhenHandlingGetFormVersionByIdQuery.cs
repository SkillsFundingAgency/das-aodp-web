using AutoFixture;
using AutoFixture.AutoMoq;
using Azure;
using Moq;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.Aodp.Application.Tests.Queries.FormBuilder.Forms
{
    public class WhenHandlingGetFormVersionByIdQuery
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private GetFormVersionByIdQueryHandler _handler;


        public WhenHandlingGetFormVersionByIdQuery()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<GetFormVersionByIdQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetFormVersionByIdQuery>();

            var response = _fixture.Create<GetFormVersionByIdQueryResponse>();

            _apiClientMock.Setup(x => x.Get<GetFormVersionByIdQueryResponse>(It.IsAny<GetFormVersionByIdApiRequest>()))
                          .ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetFormVersionByIdQueryResponse>(It.IsAny<GetFormVersionByIdApiRequest>()), Times.Once);

            //_apiClientMock.Verify(x => x.Get<GetFormVersionByIdQueryResponse>(It.Is<GetFormVersionByIdApiRequest>(r => r. == query.FormVersionId)), Times.Once);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Value.Title);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetFormVersionByIdQuery>();
            var exception = _fixture.Create<Exception>();

            _apiClientMock.Setup(x => x.Get<GetFormVersionByIdQueryResponse>(It.IsAny<GetFormVersionByIdApiRequest>()))
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
