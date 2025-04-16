using AutoFixture;
using SFA.DAS.AODP.Application.Queries.OutputFile;
using SFA.DAS.AODP.Infrastructure.File;
using Moq;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Domain.OutputFile;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.OutputFile;

public class WhenHandlingGetPreviousOutputFilesQuery
{
    private readonly Fixture _fixture = new();
    private readonly GetPreviousOutputFilesQueryHandler _handler;
    private readonly Mock<IApiClient> _apiClientMock = new();

    public WhenHandlingGetPreviousOutputFilesQuery()
    {
        _handler = new(_apiClientMock.Object);
    }

    [Fact]
    public async Task Then_The_CommandResult_Is_Returned_As_Expected()
    {
        // Arrange
        var expectedResponse = _fixture.Create<GetPreviousOutputFilesQueryResponse>();

        var request = _fixture.Create<GetPreviousOutputFilesQuery>();
        _apiClientMock
            .Setup(a => a.Get<GetPreviousOutputFilesQueryResponse>(It.IsAny<GetPreviousOutputFilesApiRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        _apiClientMock
              .Verify(a => a.Get<GetPreviousOutputFilesQueryResponse>(It.IsAny<GetPreviousOutputFilesApiRequest>()));

        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.NotNull(response.Value);
        Assert.Equal(expectedResponse, response.Value);
    }

    [Fact]
    public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
    {
        // Arrange
        var expectedException = _fixture.Create<Exception>();
        var request = _fixture.Create<GetPreviousOutputFilesQuery>();
        _apiClientMock
            .Setup(a => a.Get<GetPreviousOutputFilesQueryResponse>(It.IsAny<GetPreviousOutputFilesApiRequest>()))
            .ThrowsAsync(expectedException);


        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.NotEmpty(response.ErrorMessage!);
        Assert.Equal(expectedException.Message, response.ErrorMessage);
    }
}
