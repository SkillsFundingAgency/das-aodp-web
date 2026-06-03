using AutoFixture;
using Moq;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Application.Queries.Test;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Tests.Commands.FormBuilder.Forms
{
    public class WhenHandlingGetChangedQualificationsQuery
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly GetChangedQualificationsQueryHandler _handler;

        public WhenHandlingGetChangedQualificationsQuery()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            // Arrange
            var expectedResponse = _fixture.Create<GetChangedQualificationsQueryResponse>();
            var request = _fixture.Create<GetChangedQualificationsQuery>();

            _apiClient
                .Setup(a => a.Get<GetChangedQualificationsQueryResponse>(
                    It.IsAny<GetChangedQualificationsApiRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.NotNull(response.Value);
            Assert.Equal(expectedResponse.TotalRecords, response.Value.TotalRecords);
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var expectedException = new Exception("API failed");
            var request = _fixture.Create<GetChangedQualificationsQuery>();

            _apiClient
                .Setup(a => a.Get<GetChangedQualificationsQueryResponse>(
                    It.IsAny<GetChangedQualificationsApiRequest>()))
                .ThrowsAsync(expectedException);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            Assert.False(response.Success);
            Assert.Equal(expectedException.Message, response.ErrorMessage);
        }

        [Fact]
        public async Task Handle_Maps_All_Fields_To_ApiRequest()
        {
            // Arrange
            var processStatusIds = new List<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid()
            };

            var ageGroups = new List<AgeGroup>
            {
                AgeGroup.Pre16,
                AgeGroup.EighteenPlus
            };

            var request = new GetChangedQualificationsQuery
            {
                Skip = 10,
                Take = 20,
                Name = "Diploma",
                Organisation = "City & Guilds",
                QAN = "12345678",
                ProcessStatusIds = processStatusIds,
                AgeGroups = ageGroups
            };

            GetChangedQualificationsApiRequest? captured = null;

            _apiClient
                .Setup(a => a.Get<GetChangedQualificationsQueryResponse>(
                    It.IsAny<GetChangedQualificationsApiRequest>()))
                .Callback<object>(req => captured = (GetChangedQualificationsApiRequest)req)
                .ReturnsAsync(new GetChangedQualificationsQueryResponse());

            // Act
            await _handler.Handle(request, default);

            // Assert
            Assert.NotNull(captured);

            Assert.Equal(request.Skip, captured!.Skip);
            Assert.Equal(request.Take, captured.Take);
            Assert.Equal(request.Name, captured.Name);
            Assert.Equal(request.Organisation, captured.Organisation);
            Assert.Equal(request.QAN, captured.QAN);

            Assert.Equal(processStatusIds,
                captured.ProcessStatusFilter.ProcessStatusIds);

            Assert.Equal(ageGroups, captured.AgeGroups);
        }
    }
}