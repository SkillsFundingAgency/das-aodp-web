using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;
using System.Runtime.CompilerServices;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.Qualifications;

public class WhenGetQualificationsQueryHandlerQuery
{
    private readonly IFixture _fixture;
    private readonly Mock<IApiClient> _apiClientMock;

    public WhenGetQualificationsQueryHandlerQuery()
    {
        //_apiClientMock = new Mock<IApiClient>(MockBehavior.Strict);
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
    }

    [Fact]
    public async Task Handle_WhenRequestProvided_ReturnsDummyResponse()
    {
        // Arrange
        var handler = new GetQualificationsQueryHandler(_apiClientMock.Object);

        var request = new GetQualificationsQuery
        {
            Skip = 10,
            Take = 25,
            SearchTerm = "plumbing"
        };

        var expectedResponse = new GetQualificationsQueryResponse
        {
            TotalRecords = 5,
            Skip = request.Skip,
            Take = request.Take,
            Qualifications = new List<GetMatchingQualificationsQueryItem>
            {
                new GetMatchingQualificationsQueryItem { Qan = "QAN-0001", QualificationName = "Plumbing Level 1" },
                new GetMatchingQualificationsQueryItem { Qan = "QAN-0002", QualificationName = "Advanced Plumbing" },
                new GetMatchingQualificationsQueryItem { Qan = "QAN-0003", QualificationName = "Plumbing Technician" },
                new GetMatchingQualificationsQueryItem { Qan = "QAN-0004", QualificationName = "Plumbing Supervisor" },
                new GetMatchingQualificationsQueryItem { Qan = "QAN-0005", QualificationName = "Master Plumber" }
            }
        };

        _apiClientMock
            .Setup(a => a.Get<GetQualificationsQueryResponse>(It.IsAny<GetQualificationsApiRequest>()))
            .ReturnsAsync(expectedResponse);


        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);

        Assert.NotNull(result.Value);
        Assert.Equal(5, result.Value.TotalRecords);
        Assert.Equal(request.Skip, result.Value.Skip);
        Assert.Equal(request.Take, result.Value.Take);

        Assert.NotNull(result.Value.Qualifications);
        Assert.Equal(5, result.Value.Qualifications.Count);

        Assert.Equal("QAN-0001", result.Value.Qualifications[0].Qan);
        Assert.Contains("Plumbing", result.Value.Qualifications[0].QualificationName, StringComparison.OrdinalIgnoreCase);

    }

    [Fact]
    public async Task Handle_WhenRequestIsNull_CatchesExceptionAndReturnsErrorResponse()
    {
        // Arrange
        var apiClientMock = new Mock<IApiClient>(MockBehavior.Strict);
        var handler = new GetQualificationsQueryHandler(apiClientMock.Object);

        // Act
        var result = await handler.Handle(null!, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));

        // Asert
        Assert.NotNull(result.Value);
        Assert.Equal(0, result.Value.TotalRecords);
        Assert.Equal(0, result.Value.Skip);
        Assert.Equal(0, result.Value.Take);
        Assert.True(result.Value.Qualifications == null || result.Value.Qualifications.Count == 0);

        apiClientMock.VerifyNoOtherCalls();
    }
}
