using AutoFixture;
using Moq;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Tests.Queries.Qualifications
{
    public class WhenHandlingGetQualificationOutputFileQuery
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly GetQualificationOutputFileQueryHandler _handler;

        public WhenHandlingGetQualificationOutputFileQuery()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            // Arrange
            var expectedResponse = _fixture.Build<GetQualificationOutputFileResponse>()
                                           .With(r => r.FileName, "export.zip")
                                           .With(r => r.ZipFileContent, new byte[] { 1, 2, 3 })
                                           .With(r => r.ContentType, "application/zip")
                                           .Create();
            var request = _fixture.Create<GetQualificationOutputFileQuery>();

            _apiClient
                .Setup(a => a.Get<GetQualificationOutputFileResponse>(It.IsAny<GetQualificationOutputFileApiRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            _apiClient.Verify(a => a.Get<GetQualificationOutputFileResponse>(
                                  It.IsAny<GetQualificationOutputFileApiRequest>()),
                              Times.Once);

            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.NotNull(response.Value);
            Assert.Equal("export.zip", response.Value.FileName);
            Assert.NotNull(response.Value.ZipFileContent);
            Assert.NotEmpty(response.Value.ZipFileContent!);
            Assert.Equal("application/zip", response.Value.ContentType);
        }

        [Fact]
        public async Task And_Api_Returns_EmptyFile_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange: empty bytes triggers failure path
            var emptyResponse = _fixture.Build<GetQualificationOutputFileResponse>()
                                        .With(r => r.FileName, "export.zip")
                                        .With(r => r.ZipFileContent, Array.Empty<byte>())
                                        .With(r => r.ContentType, "application/zip")
                                        .Create();
            var request = _fixture.Create<GetQualificationOutputFileQuery>();

            _apiClient
                .Setup(a => a.Get<GetQualificationOutputFileResponse>(It.IsAny<GetQualificationOutputFileApiRequest>()))
                .ReturnsAsync(emptyResponse);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("Output file not available.", response.ErrorMessage);
            Assert.Null(response.Value.FileName);
            Assert.Null(response.Value.ZipFileContent);
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var expectedException = _fixture.Create<Exception>();
            var request = _fixture.Create<GetQualificationOutputFileQuery>();

            _apiClient
                .Setup(a => a.Get<GetQualificationOutputFileResponse>(It.IsAny<GetQualificationOutputFileApiRequest>()))
                .ThrowsAsync(expectedException);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.NotEmpty(response.ErrorMessage!);
            Assert.Equal(expectedException.Message, response.ErrorMessage);
            Assert.Null(response.Value.ZipFileContent);
            Assert.Null(response.Value.FileName);
        }
    }
}