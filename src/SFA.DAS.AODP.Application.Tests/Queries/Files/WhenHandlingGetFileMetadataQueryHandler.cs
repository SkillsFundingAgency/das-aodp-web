using AutoFixture;
using Moq;
using SFA.DAS.AODP.Application.Queries.Files.Get;
using SFA.DAS.AODP.Domain.Files;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Tests.Queries.Files
{
    public class WhenHandlingGetFileMetadataQuery
    {
        private const string ExpectedErrorMessage = "api failed";

        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly GetFileMetadataQueryHandler _handler;

        public WhenHandlingGetFileMetadataQuery()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_QueryResult_Is_Returned_As_Expected()
        {
            // Arrange
            var request = _fixture.Create<GetFileMetadataQuery>();

            var expectedResponse = _fixture.Create<GetFileMetadataQueryResponse>();

            _apiClient
                .Setup(a => a.PostWithResponseCode<GetFileMetadataQueryResponse>(
                    It.IsAny<GetFileMetadataApiRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            Assert.Multiple(() =>
            {
                _apiClient.Verify(a =>
                    a.PostWithResponseCode<GetFileMetadataQueryResponse>(
                        It.Is<GetFileMetadataApiRequest>(r => r.Data == request)),
                    Times.Once);

                Assert.NotNull(response);
                Assert.True(response.Success);
                Assert.NotNull(response.Value);
                Assert.Equal(expectedResponse, response.Value);
            });
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailQueryResult_Is_Returned()
        {
            // Arrange
            var request = _fixture.Create<GetFileMetadataQuery>();
            var expectedException = new Exception(ExpectedErrorMessage);

            _apiClient
                .Setup(a => a.PostWithResponseCode<GetFileMetadataQueryResponse>(
                    It.IsAny<GetFileMetadataApiRequest>()))
                .ThrowsAsync(expectedException);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            Assert.Multiple(() =>
            {
                _apiClient.Verify(a =>
                    a.PostWithResponseCode<GetFileMetadataQueryResponse>(
                        It.Is<GetFileMetadataApiRequest>(r => r.Data == request)),
                    Times.Once);

                Assert.NotNull(response);
                Assert.False(response.Success);
                Assert.Equal(ExpectedErrorMessage, response.ErrorMessage);
                Assert.NotNull(response.Value); // consistent with project conventions
            });
        }
    }
}