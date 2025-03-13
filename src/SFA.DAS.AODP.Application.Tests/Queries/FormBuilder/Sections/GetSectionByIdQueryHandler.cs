using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Sections;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.Aodp.UnitTests.Application.Queries.FormBuilder.Sections
{
    public class GetSectionByIdQueryHandlerTests
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private GetSectionByIdQueryHandler _handler;

        public GetSectionByIdQueryHandlerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<GetSectionByIdQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetSectionByIdQuery>();

            var response = _fixture.Create<GetSectionByIdQueryResponse>();

            _apiClientMock.Setup(x => x.Get<GetSectionByIdQueryResponse>(It.IsAny<GetSectionByIdApiRequest>()))
                          .ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetSectionByIdQueryResponse>(It.IsAny<GetSectionByIdApiRequest>()), Times.Once);

            //_apiClientMock.Verify(x => x.Get<GetSectionByIdQueryResponse>(It.Is<GetSectionByIdApiRequest>(r => r == query.FormVersionId)), Times.Once);

            Assert.True(result.Success);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetSectionByIdQuery>();
            var exception = _fixture.Create<Exception>();

            _apiClientMock.Setup(x => x.Get<GetSectionByIdQueryResponse>(It.IsAny<GetSectionByIdApiRequest>()))
                          .ThrowsAsync(exception);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(exception.Message, result.ErrorMessage);
        }
    }
}
