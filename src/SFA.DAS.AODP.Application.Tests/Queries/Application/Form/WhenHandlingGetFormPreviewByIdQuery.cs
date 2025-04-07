using AutoFixture;
using AutoFixture.AutoMoq;
using Azure;
using Moq;
using SFA.DAS.AODP.Application.Queries.Application.Form;
using SFA.DAS.AODP.Domain.Application.Form;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.Aodp.Application.Tests.Queries.Application.Form
{
    public class WhenHandlingGetFormPreviewByIdQuery
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private GetFormPreviewByIdQueryHandler _handler;


        public WhenHandlingGetFormPreviewByIdQuery()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<GetFormPreviewByIdQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetFormPreviewByIdQuery>();

            var response = _fixture.Create<GetFormPreviewByIdQueryResponse>();

            _apiClientMock.Setup(x => x.Get<GetFormPreviewByIdQueryResponse>(It.IsAny<GetFormPreviewByIdApiRequest>()))
                          .ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetFormPreviewByIdQueryResponse>(It.IsAny<GetFormPreviewByIdApiRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetFormPreviewByIdQueryResponse>(It.Is<GetFormPreviewByIdApiRequest>(r => r._applicationId == query.ApplicationId)), Times.Once);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Value.ApplicationId);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetFormPreviewByIdQuery>();
            var exception = _fixture.Create<Exception>();

            _apiClientMock.Setup(x => x.Get<GetFormPreviewByIdQueryResponse>(It.IsAny<GetFormPreviewByIdApiRequest>()))
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
