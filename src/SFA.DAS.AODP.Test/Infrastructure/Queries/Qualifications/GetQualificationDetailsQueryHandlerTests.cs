using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Qualifications;

namespace SFA.DAS.AODP.Test.Infrastructure.Queries.Qualifications;

public class GetQualificationDetailsQueryHandlerTests
{
    private readonly Mock<IApiClient> _apiClientMock;
    private readonly GetQualificationDetailsQueryHandler _handler;

    public GetQualificationDetailsQueryHandlerTests()
    {
        _apiClientMock = new Mock<IApiClient>();
        _handler = new GetQualificationDetailsQueryHandler(_apiClientMock.Object);
    }

    [Fact]
    public async Task Then_The_Api_Is_Called_With_The_Request_And_QualificationDetailsData_Is_Returned()
    {
        // Arrange
        var query = new GetQualificationDetailsQuery { Id = 1 };
        var response = new GetQualificationDetailsQueryResponse
        {
            Success = true,
            Id = 1,
            Status = "Active",
            Priority = "High",
            Changes = "None",
            QualificationReference = "Ref123",
            AwardingOrganisation = "Org1",
            Title = "Qualification 1",
            QualificationType = "Type1",
            Level = "Level1",
            ProposedChanges = "None",
            AgeGroup = "18+",
            Category = "Category1",
            Subject = "Subject1",
            SectorSubjectArea = "Area1",
            Comments = "No comments"
        };
        _apiClientMock.Setup(x => x.Get<GetQualificationDetailsQueryResponse>(It.IsAny<GetQualificationDetailsApiRequest>()))
                      .ReturnsAsync(response);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _apiClientMock.Verify(x => x.Get<GetQualificationDetailsQueryResponse>(It.IsAny<GetQualificationDetailsApiRequest>()), Times.Once);
        Assert.True(result.Success);
        Assert.Equal(1, result.Id);
        Assert.Equal("Active", result.Status);
    }

    [Fact]
    public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
    {
        // Arrange
        var query = new GetQualificationDetailsQuery { Id = 1 };
        _apiClientMock.Setup(x => x.Get<GetQualificationDetailsQueryResponse?>(It.IsAny<GetQualificationDetailsApiRequest>()))
                      .ReturnsAsync((GetQualificationDetailsQueryResponse?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _apiClientMock.Verify(x => x.Get<GetQualificationDetailsQueryResponse>(It.IsAny<GetQualificationDetailsApiRequest>()), Times.Once);
        Assert.False(result.Success);
    }
}

