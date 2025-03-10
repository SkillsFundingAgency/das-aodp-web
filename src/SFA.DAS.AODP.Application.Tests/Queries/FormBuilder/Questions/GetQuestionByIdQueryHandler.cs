using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Questions;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.Aodp.UnitTests.Application.Queries.FormBuilder.Questions
{
    public class GetQuestionByIdQueryHandlerTests
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private GetQuestionByIdQueryHandler _handler;

        public GetQuestionByIdQueryHandlerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<GetQuestionByIdQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_ApplicationFormData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetQuestionByIdQuery>();

            //var response = _fixture.Create<GetQuestionByIdQueryResponse>();

            var response = new GetQuestionByIdQueryResponse()
            {
                Id = Guid.NewGuid(),
                PageId = Guid.NewGuid(),
                Title = "test",
                Key = Guid.NewGuid(),
                Hint = "test",
                Order = 1,
                Required = false,
                Type = "test",
                Helper = "test",
                HelperHTML = "test"
            };

            _apiClientMock.Setup(x => x.Get<GetQuestionByIdQueryResponse>(It.IsAny<GetQuestionByIdApiRequest>()))
                          .ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetQuestionByIdQueryResponse>(It.IsAny<GetQuestionByIdApiRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetQuestionByIdQueryResponse>(It.Is<GetQuestionByIdApiRequest>(r => r.FormVersionId == query.FormVersionId)), Times.Once);

            Assert.True(result.Success);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetQuestionByIdQuery>();
            var exception = _fixture.Create<Exception>();

            _apiClientMock.Setup(x => x.Get<GetQuestionByIdQueryResponse>(It.IsAny<GetQuestionByIdApiRequest>()))
                          .ThrowsAsync(exception);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(exception.Message, result.ErrorMessage);
        }
    }
}
