using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Application.Queries.Application.Review;
using SFA.DAS.AODP.Domain.Application.Review;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.UnitTests.Helper;

namespace SFA.DAS.AODP.UnitTests.Application.Queries.Application.Review
{
    public class GetApplicationExportDetailsQueryHandlerTests
    {
        private IFixture? _fixture;
        private Mock<IApiClient>? _apiClientMock;
        private GetApplicationExportDetailsQueryHandler _handler;

        public GetApplicationExportDetailsQueryHandlerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Customizations.Add(new DateOnlySpecimenBuilder());

            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = new GetApplicationExportDetailsQueryHandler(_apiClientMock.Object);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_With_The_Request_And_Data_Is_Returned()
        {
            var query = _fixture.Create<GetApplicationExportDataQuery>();
            var response = _fixture.Create<GetApplicationExportDataQueryResponse>();

            _apiClientMock.Setup(x =>
                x.Get<GetApplicationExportDataQueryResponse>(
                    It.IsAny<GetApplicationExportDetailsApiRequest>()))
                .ReturnsAsync(response);

            var result = await _handler.Handle(query, CancellationToken.None);

            _apiClientMock.Verify(x =>
                x.Get<GetApplicationExportDataQueryResponse>(
                    It.IsAny<GetApplicationExportDetailsApiRequest>()), Times.Once);

            _apiClientMock.Verify(x =>
                x.Get<GetApplicationExportDataQueryResponse>(
                    It.Is<GetApplicationExportDetailsApiRequest>(r =>
                        r.ApplicationReviewId == query.ApplicationReviewId)), Times.Once);

            Assert.True(result.Success);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Then_The_Api_Is_Called_And_Failure_Is_Returned()
        {
            var query = _fixture.Create<GetApplicationExportDataQuery>();
            var exception = new Exception("API failed");

            _apiClientMock.Setup(x =>
                x.Get<GetApplicationExportDataQueryResponse>(
                    It.IsAny<GetApplicationExportDetailsApiRequest>()))
                .ThrowsAsync(exception);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.False(result.Success);
            Assert.Equal(exception.Message, result.ErrorMessage);
        }
    }
}