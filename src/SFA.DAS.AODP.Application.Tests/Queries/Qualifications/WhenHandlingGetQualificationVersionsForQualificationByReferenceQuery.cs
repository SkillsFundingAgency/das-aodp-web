using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using Moq;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Tests.Queries.Qualifications
{
    public class WhenHandlingGetQualificationVersionsForQualificationByReferenceQuery
    {
        private readonly IFixture _fixture;
        private readonly Mock<IApiClient> _apiClientMock;
        private readonly GetQualificationVersionsForQualificationByReferenceQueryHandler _handler;

        public WhenHandlingGetQualificationVersionsForQualificationByReferenceQuery()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Customizations.Add(new DateOnlySpecimenBuilder());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = new GetQualificationVersionsForQualificationByReferenceQueryHandler(_apiClientMock.Object);
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
            var expectedResponse = _fixture.Create<BaseMediatrResponse<GetQualificationVersionsForQualificationByReferenceQueryResponse>>();
            var request = _fixture.Create<GetQualificationVersionsForQualificationByReferenceQuery>();
            _apiClientMock
                .Setup(a => a.Get<BaseMediatrResponse<GetQualificationVersionsForQualificationByReferenceQueryResponse>>(It.IsAny<GetQualificationVersionsForQualificationByReferenceApiRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _handler.Handle(request, CancellationToken.None);

            // Assert
            _apiClientMock
                .Verify(a => a.Get<BaseMediatrResponse<GetQualificationVersionsForQualificationByReferenceQueryResponse>>(It.Is<GetQualificationVersionsForQualificationByReferenceApiRequest>(r => r.QualificationReference == request.QualificationReference)));

            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.NotNull(response.Value);
            Assert.Equal(expectedResponse.Value, response.Value);
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var expectedException = _fixture.Create<Exception>();
            var request = _fixture.Create<GetQualificationVersionsForQualificationByReferenceQuery>();
            _apiClientMock
                .Setup(a => a.Get<BaseMediatrResponse<GetQualificationVersionsForQualificationByReferenceQueryResponse>>(It.IsAny<GetQualificationVersionsForQualificationByReferenceApiRequest>()))
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
