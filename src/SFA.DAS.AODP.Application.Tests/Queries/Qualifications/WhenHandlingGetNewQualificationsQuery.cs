using AutoFixture;
using Moq;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Application.Queries.Test;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Tests.Commands.FormBuilder.Forms
{
    public class WhenHandlingGetNewQualificationsQuery
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly GetNewQualificationsQueryHandler _handler;


        public WhenHandlingGetNewQualificationsQuery()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            // Arrange
            var expectedResponse = _fixture.Create<GetNewQualificationsQueryResponse>();
            var request = _fixture.Create<GetNewQualificationsQuery>();
            _apiClient
                .Setup(a => a.Get<GetNewQualificationsQueryResponse>(It.IsAny<GetNewQualificationsApiRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            _apiClient
                .Verify(a => a.Get<GetNewQualificationsQueryResponse>(It.Is<GetNewQualificationsApiRequest>(r => r.Skip == request.Skip)));

            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.NotNull(response.Value);
            Assert.Equal(expectedResponse.TotalRecords, response.Value.TotalRecords);
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var expectedException = _fixture.Create<Exception>();
            var request = _fixture.Create<GetNewQualificationsQuery>();
            _apiClient
                .Setup(a => a.Get<GetNewQualificationsQueryResponse>(It.IsAny<GetNewQualificationsApiRequest>()))
                .ThrowsAsync(expectedException);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.NotEmpty(response.ErrorMessage!);
            Assert.Equal(expectedException.Message, response.ErrorMessage);
        }
    }
}