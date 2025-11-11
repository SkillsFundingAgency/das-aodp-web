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

        private const string OutputFileError = "output file error goes here";

        public WhenHandlingGetQualificationOutputFileQuery()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            // Arrange
            var payload = _fixture.Build<GetQualificationOutputFileResponse>()
                                  .With(r => r.FileName, "export.zip")
                                  .With(r => r.ZipFileContent, new byte[] { 1, 2, 3 })
                                  .With(r => r.ContentType, "application/zip")
                                  .Create();

            var outerEnvelope = new BaseMediatrResponse<GetQualificationOutputFileResponse>
            {
                Success = true,
                Value = payload
            };

            var request = _fixture.Create<GetQualificationOutputFileQuery>();

            _apiClient
                .Setup(a => a.PostWithResponseCode<BaseMediatrResponse<GetQualificationOutputFileResponse>>(
                    It.IsAny<GetQualificationOutputFileApiRequest>()))
                .ReturnsAsync(outerEnvelope);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            _apiClient.Verify(a => a.PostWithResponseCode<BaseMediatrResponse<GetQualificationOutputFileResponse>>(
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
        public async Task And_Api_Returns_FailureEnvelope_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var outerEnvelope = new BaseMediatrResponse<GetQualificationOutputFileResponse>
            {
                Success = false,
                ErrorMessage = OutputFileError,
            };

            var request = _fixture.Create<GetQualificationOutputFileQuery>();

            _apiClient
                .Setup(a => a.PostWithResponseCode<BaseMediatrResponse<GetQualificationOutputFileResponse>>(
                    It.IsAny<GetQualificationOutputFileApiRequest>()))
                .ReturnsAsync(outerEnvelope);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal(OutputFileError, response.ErrorMessage);
            Assert.True(response.Value is null ||
                        (response.Value.FileName is null && response.Value.ZipFileContent is null));
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var expectedException = _fixture.Create<Exception>();
            var request = _fixture.Create<GetQualificationOutputFileQuery>();

            _apiClient
                .Setup(a => a.PostWithResponseCode<BaseMediatrResponse<GetQualificationOutputFileResponse>>(
                    It.IsAny<GetQualificationOutputFileApiRequest>()))
                .ThrowsAsync(expectedException);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal(expectedException.Message, response.ErrorMessage);
            Assert.True(response.Value is null ||
                        (response.Value.FileName is null && response.Value.ZipFileContent is null));
        }
    }
}
