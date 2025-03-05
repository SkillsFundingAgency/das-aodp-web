using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Application.Queries.Test;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;
using Xunit;

namespace SFA.DAS.AODP.Infrastructure.Tests.Queries.Qualifications;

public class GetNewQualificationsQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IApiClient> _apiClientMock;
    private readonly GetNewQualificationsQueryHandler _handler;

    public GetNewQualificationsQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
        _handler = _fixture.Create<GetNewQualificationsQueryHandler>();
    }

    [Fact]
    public async Task Then_The_Api_Is_Called_With_The_Request_And_NewQualificationsData_Is_Returned()
    {
        // Arrange
        var query = _fixture.Create<GetNewQualificationsQuery>();
        var response = _fixture.Create<BaseMediatrResponse<GetNewQualificationsQueryResponse>>();
        response.Success = true;
        response.Value.Data = _fixture.CreateMany<NewQualification>(2).ToList();

        _apiClientMock.Setup(x => x.Get<BaseMediatrResponse<GetNewQualificationsQueryResponse>>(It.IsAny<GetNewQualificationsApiRequest>()))
                      .ReturnsAsync(response);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _apiClientMock.Verify(x => x.Get<BaseMediatrResponse<GetNewQualificationsQueryResponse>>(It.IsAny<GetNewQualificationsApiRequest>()), Times.Once);
        Assert.True(result.Success);
        Assert.Equal(2, result.Value.Data.Count);
    }

    [Fact]
    public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
    {
        // Arrange
        var query = _fixture.Create<GetNewQualificationsQuery>();
        var response = _fixture.Create<BaseMediatrResponse<GetNewQualificationsQueryResponse>>();
        response.Success = false;
        response.Value = null;

        _apiClientMock.Setup(x => x.Get<BaseMediatrResponse<GetNewQualificationsQueryResponse>>(It.IsAny<GetNewQualificationsApiRequest>()))
                      .ReturnsAsync(response);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _apiClientMock.Verify(x => x.Get<BaseMediatrResponse<GetNewQualificationsQueryResponse>>(It.IsAny<GetNewQualificationsApiRequest>()), Times.Once);
        Assert.False(result.Success);
        Assert.Equal("No new qualifications found.", result.ErrorMessage);
    }

    [Fact]
    public async Task Then_The_Api_Is_Called_With_The_Request_And_Exception_Is_Handled()
    {
        // Arrange
        var query = _fixture.Create<GetNewQualificationsQuery>();
        var exceptionMessage = "An error occurred";
        _apiClientMock.Setup(x => x.Get<BaseMediatrResponse<GetNewQualificationsQueryResponse>>(It.IsAny<GetNewQualificationsApiRequest>()))
                      .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _apiClientMock.Verify(x => x.Get<BaseMediatrResponse<GetNewQualificationsQueryResponse>>(It.IsAny<GetNewQualificationsApiRequest>()), Times.Once);
        Assert.False(result.Success);
        Assert.Equal(exceptionMessage, result.ErrorMessage);
    }
}


