﻿using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Infrastructure.Tests.Queries.Qualifications;

public class GetQualificationDetailsQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IApiClient> _apiClientMock;
    private readonly GetQualificationDetailsQueryHandler _handler;

    public GetQualificationDetailsQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
         .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
        _handler = _fixture.Create<GetQualificationDetailsQueryHandler>();
    }

    [Fact]
    public async Task Then_The_Api_Is_Called_With_The_Request_And_QualificationDetailsData_Is_Returned()
    {
        // Arrange
        var query = _fixture.Create<GetQualificationDetailsQuery>();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
.ToList()
.ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        var response = _fixture.Create<GetQualificationDetailsQueryResponse>();

        _apiClientMock.Setup(x => x.Get<GetQualificationDetailsQueryResponse>(It.IsAny<GetQualificationDetailsApiRequest>()))
                      .ReturnsAsync(response);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _apiClientMock.Verify(x => x.Get<GetQualificationDetailsQueryResponse>(It.IsAny<GetQualificationDetailsApiRequest>()), Times.Once);
        Assert.True(result.Success);
        Assert.Equal(response.Id, result.Value.Id);
    }

    [Fact]
    public async Task Then_The_Api_Is_Called_With_The_Request_And_Failure_Is_Returned()
    {
        // Arrange
        var query = _fixture.Create<GetQualificationDetailsQuery>();

        _apiClientMock.Setup(x => x.Get<GetQualificationDetailsQueryResponse>(It.IsAny<GetQualificationDetailsApiRequest>()))
                      .Returns(Task.FromResult<GetQualificationDetailsQueryResponse>(null));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _apiClientMock.Verify(x => x.Get<GetQualificationDetailsQueryResponse>(It.IsAny<GetQualificationDetailsApiRequest>()), Times.Once);
        Assert.False(result.Success);
        Assert.Equal($"No details found for qualification reference: {query.QualificationReference}", result.ErrorMessage);
    }

    [Fact]
    public async Task Then_The_Api_Is_Called_With_The_Request_And_Exception_Is_Handled()
    {
        // Arrange
        var query = _fixture.Create<GetQualificationDetailsQuery>();
        var exceptionMessage = "An error occurred";
        _apiClientMock.Setup(x => x.Get<GetQualificationDetailsQueryResponse>(It.IsAny<GetQualificationDetailsApiRequest>()))
                      .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _apiClientMock.Verify(x => x.Get<GetQualificationDetailsQueryResponse>(It.IsAny<GetQualificationDetailsApiRequest>()), Times.Once);
        Assert.False(result.Success);
        Assert.Equal(exceptionMessage, result.ErrorMessage);
    }
}
