using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using Moq;
using SFA.DAS.AODP.Application.Queries.Review;
using SFA.DAS.AODP.Domain.Application.Review;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Infrastructure.File;

namespace SFA.DAS.AODP.Application.Tests.Queries.Qualifications
{
    public class WhenHandlingGetApplicationFormByReviewIdQuery
    {
        private readonly IFixture _fixture;
        private readonly Mock<IApiClient> _apiClientMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly GetApplicationFormByReviewIdQueryHandler _handler;

        public WhenHandlingGetApplicationFormByReviewIdQuery()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Customizations.Add(new DateOnlySpecimenBuilder());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _fileServiceMock = _fixture.Freeze<Mock<IFileService>>();
            _handler = new GetApplicationFormByReviewIdQueryHandler(_apiClientMock.Object, _fileServiceMock.Object);
        }

        public class DateOnlySpecimenBuilder : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (request is Type type && type == typeof(DateOnly))
                {
                    return new DateOnly(2023, 1, 1); // a valid date
                }

                return new NoSpecimen();
            }
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            // Arrange
            var expectedResponse = _fixture.Create<GetApplicationFormAnswersByReviewIdApiResponse>();
            var request = _fixture.Create<GetApplicationFormByReviewIdQuery>();
            _apiClientMock
                .Setup(a => a.Get<GetApplicationFormAnswersByReviewIdApiResponse>(It.IsAny<GetApplicationFormAnswersByReviewIdApiRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _handler.Handle(request, CancellationToken.None);

            // Assert
            _apiClientMock
                .Verify(a => a.Get<GetApplicationFormAnswersByReviewIdApiResponse>(It.Is<GetApplicationFormAnswersByReviewIdApiRequest>(r => r.ApplicationReviewId == request.ApplicationReviewId)));

            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.NotNull(response.Value);
            Assert.Equal(expectedResponse.ApplicationId, response.Value.ApplicationId);
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var expectedException = _fixture.Create<Exception>();
            var request = _fixture.Create<GetApplicationFormByReviewIdQuery>();
            _apiClientMock
                .Setup(a => a.Get<GetApplicationFormAnswersByReviewIdApiResponse>(It.IsAny<GetApplicationFormAnswersByReviewIdApiRequest>()))
                .ThrowsAsync(expectedException);

            // Act
            var response = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.NotEmpty(response.ErrorMessage!);
            Assert.Equal(expectedException.Message, response.ErrorMessage);
        }
    }
}
