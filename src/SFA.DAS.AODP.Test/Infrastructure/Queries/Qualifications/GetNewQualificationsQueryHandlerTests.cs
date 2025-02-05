using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Qualifications;
using SFA.DAS.AODP.Application.Queries.Test;

namespace SFA.DAS.AODP.Test.Infrastructure.Queries.Qualifications;

public class GetNewQualificationsQueryHandlerTests
{
    private readonly Mock<IApiClient> _apiClientMock;
    private readonly GetNewQualificationsQueryHandler _handler;

    public GetNewQualificationsQueryHandlerTests()
    {
        _apiClientMock = new Mock<IApiClient>();
        _handler = new GetNewQualificationsQueryHandler(_apiClientMock.Object);
    }

    [Fact]
    public async Task Then_The_Api_Is_Called_With_The_Request_And_NewQualificationsData_Is_Returned()
    {
        // Arrange
        var query = new GetNewQualificationsQuery();
        var response = new GetNewQualificationsQueryResponse
        {
            Success = true,
            NewQualifications = new List<NewQualification>
            {
                new NewQualification { Id = 1, Title = "Qualification 1" },
                new NewQualification { Id = 2, Title = "Qualification 2" }
            }
        };
        _apiClientMock.Setup(x => x.Get<GetNewQualificationsQueryResponse>(It.IsAny<GetNewQualificationsApiRequest>()))
                      .ReturnsAsync(response);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _apiClientMock.Verify(x => x.Get<GetNewQualificationsQueryResponse>(It.IsAny<GetNewQualificationsApiRequest>()), Times.Once);
        Assert.True(result.Success);
        Assert.Equal(2, result.NewQualifications.Count);
    }

    [Fact]
    public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
    {
        // Arrange
        var query = new GetNewQualificationsQuery();
        _apiClientMock.Setup(x => x.Get<GetNewQualificationsQueryResponse?>(It.IsAny<GetNewQualificationsApiRequest>()))
                      .ReturnsAsync((GetNewQualificationsQueryResponse?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _apiClientMock.Verify(x => x.Get<GetNewQualificationsQueryResponse>(It.IsAny<GetNewQualificationsApiRequest>()), Times.Once);
        Assert.False(result.Success);
    }
}
