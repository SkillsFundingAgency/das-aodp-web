using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using Azure;
using Moq;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Infrastructure.Cache;

namespace SFA.DAS.Aodp.UnitTests.Application.Queries.Application.Section
{
    public class GetQfauFeedbackForApplicationReviewConfirmationQueryHandlerTests
    {
        private IFixture? _fixture;
        private Mock<IApiClient> _apiClientMock;
        private GetQfauFeedbackForApplicationReviewConfirmationQueryHandler _handler;

        public GetQfauFeedbackForApplicationReviewConfirmationQueryHandlerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Customizations.Add(new DateOnlySpecimenBuilder());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = _fixture.Create<GetQfauFeedbackForApplicationReviewConfirmationQueryHandler>();
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_FeedbackData_By_Id_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetQfauFeedbackForApplicationReviewConfirmationQuery>();

            var response = _fixture.Create<GetQfauFeedbackForApplicationReviewConfirmationQueryResponse>();

            _apiClientMock.Setup(x => x.Get<GetQfauFeedbackForApplicationReviewConfirmationQueryResponse>(It.IsAny<GetQfauFeedbackForApplicationReviewConfirmationApiRequest>()))
                          .ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _apiClientMock.Verify(x => x.Get<GetQfauFeedbackForApplicationReviewConfirmationQueryResponse>(It.IsAny<GetQfauFeedbackForApplicationReviewConfirmationApiRequest>()), Times.Once);

            _apiClientMock.Verify(x => x.Get<GetQfauFeedbackForApplicationReviewConfirmationQueryResponse>(It.Is<GetQfauFeedbackForApplicationReviewConfirmationApiRequest>(r => r.ApplicationReviewId == query.ApplicationReviewId)), Times.Once);

            Assert.True(result.Success);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
        {
            // Arrange
            var query = _fixture.Create<GetQfauFeedbackForApplicationReviewConfirmationQuery>();
            var exception = _fixture.Create<Exception>();

            _apiClientMock.Setup(x => x.Get<GetQfauFeedbackForApplicationReviewConfirmationQueryResponse>(It.IsAny<GetQfauFeedbackForApplicationReviewConfirmationApiRequest>()))
              .ThrowsAsync(exception);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(exception.Message, result.ErrorMessage);
        }

        public class DateOnlySpecimenBuilder : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (request is Type type && type == typeof(DateOnly))
                {
                    return DateOnly.FromDateTime(DateTime.Now);
                }

                return new NoSpecimen();
            }
        }
    }
}
