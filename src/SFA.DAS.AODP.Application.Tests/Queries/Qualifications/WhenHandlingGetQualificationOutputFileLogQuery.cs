using AutoFixture;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Moq;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;
using Xunit;
using static SFA.DAS.AODP.Application.Queries.Qualifications.GetQualificationOutputFileLogResponse;

namespace SFA.DAS.AODP.Application.Tests.Queries.Qualifications
{
    public class WhenHandlingGetQualificationOutputFileLogQuery
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly GetQualificationOutputFileLogQueryHandler _handler;

        private const string noLogsmessage = "No output file logs available.";

        public WhenHandlingGetQualificationOutputFileLogQuery()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            // Arrange
            var logs = _fixture.CreateMany<QualificationOutputFileLog>(2).ToList();
            var expectedResponse = _fixture.Build<GetQualificationOutputFileLogResponse>()
                                           .With(r => r.OutputFileLogs, logs)
                                           .Create();
            var request = _fixture.Create<GetQualificationOutputFileLogQuery>();


            _apiClient
                .Setup(a => a.Get<BaseMediatrResponse<GetQualificationOutputFileLogResponse>>(It.IsAny<GetQualificationOutputFileLogApiRequest>()))
                .ReturnsAsync(new BaseMediatrResponse<GetQualificationOutputFileLogResponse>
                {
                    Success = true,
                    Value = expectedResponse
                });


            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            _apiClient.Verify(a => a.Get<BaseMediatrResponse<GetQualificationOutputFileLogResponse>>(
                                  It.IsAny<GetQualificationOutputFileLogApiRequest>()),
                              Times.Once);


            Assert.Multiple(() =>
            {
                Assert.NotNull(response);
                Assert.True(response.Success);
                Assert.NotNull(response.Value);
                Assert.NotNull(response.Value.OutputFileLogs);
                Assert.NotEmpty(response.Value.OutputFileLogs);
                Assert.Equal(2, response.Value.OutputFileLogs.Count());
            });
        }

        [Fact]
        public async Task And_Api_Returns_Empty_Logs_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var emptyLogs = new List<QualificationOutputFileLog>();
            var emptyResponse = _fixture.Build<GetQualificationOutputFileLogResponse>()
                                        .With(r => r.OutputFileLogs, emptyLogs)
                                        .Create();
            var request = _fixture.Create<GetQualificationOutputFileLogQuery>();

            _apiClient
                .Setup(a => a.Get<GetQualificationOutputFileLogResponse>(It.IsAny<GetQualificationOutputFileLogApiRequest>()))
                .ReturnsAsync(emptyResponse);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(response);
                Assert.False(response.Success);
                Assert.Equal(noLogsmessage, response.ErrorMessage);
                Assert.NotNull(response.Value);
            });
        }

        [Fact]
        public async Task And_Api_Returns_Null_Response_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var request = _fixture.Create<GetQualificationOutputFileLogQuery>();

            _apiClient
                .Setup(a => a.Get<GetQualificationOutputFileLogResponse>(It.IsAny<GetQualificationOutputFileLogApiRequest>()))
                .ReturnsAsync((GetQualificationOutputFileLogResponse)null!);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(response);
                Assert.False(response.Success);
                Assert.Equal(noLogsmessage, response.ErrorMessage);
                Assert.NotNull(response.Value);
            });
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var expectedException = _fixture.Create<Exception>();
            var request = _fixture.Create<GetQualificationOutputFileLogQuery>();

            _apiClient
                .Setup(a => a.Get<GetQualificationOutputFileLogResponse>(It.IsAny<GetQualificationOutputFileLogApiRequest>()))
                .ThrowsAsync(expectedException);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(response);
                Assert.False(response.Success);
                Assert.NotEmpty(response.ErrorMessage!);
                Assert.Equal(noLogsmessage, response.ErrorMessage);
                Assert.NotNull(response.Value);
            });
        }
    }
}
