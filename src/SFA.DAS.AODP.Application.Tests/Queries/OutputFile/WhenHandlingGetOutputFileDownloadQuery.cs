using AutoFixture;
using SFA.DAS.AODP.Application.Queries.OutputFile;
using SFA.DAS.AODP.Infrastructure.File;
using Moq;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.OutputFile;

public class WhenHandlingGetOutputFileDownloadQuery
{
    private readonly Fixture _fixture = new();
    private readonly GetOutputFileDownloadQueryHandler _handler;
    private readonly Mock<IFileService> _fileService = new Mock<IFileService>();

    public WhenHandlingGetOutputFileDownloadQuery()
    {
        _handler = new(_fileService.Object);
    }

    [Fact]
    public async Task Then_The_CommandResult_Is_Returned_As_Expected()
    {
        var request = _fixture.Create<GetOutputFileDownloadQuery>();
        var expectedResult = new Mock<GetOutputFileDownloadQueryResponse>();
        var expectedResultStream = new Mock<Stream>();
        expectedResult.Object.ContentType = "text/plain";
        expectedResult.Object.FileStream = expectedResultStream.Object;

        _fileService.Setup(v => v.OpenReadStreamAsync(It.Is<string>(s => s == request.FileName)))
            .Returns(Task.FromResult(expectedResult.Object.FileStream));
        _fileService.Setup(v => v.GetBlobContentType(It.Is<string>(s => s == request.FileName)))
            .Returns(expectedResult.Object.ContentType);

        var response = await _handler.Handle(request, new());

        Assert.True(response.Success);
        Assert.NotNull(response.Value);
        Assert.Equal(expectedResult.Object.ContentType, response.Value.ContentType);
        Assert.NotNull(response.Value.FileStream);
    }

    [Fact]
    public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
    {
        var expectedException = _fixture.Create<Exception>();
        var request = _fixture.Create<GetOutputFileDownloadQuery>();
        var streamMock = new Mock<Stream>();

        _fileService.Setup(v => v.OpenReadStreamAsync(It.Is<string>(s => s == request.FileName)))
            .Throws(expectedException);
        _fileService.Setup(v => v.GetBlobContentType(It.Is<string>(s => s == request.FileName)))
            .Returns("helloworld");

        var response = await _handler.Handle(request, new());

        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.NotEmpty(response.ErrorMessage!);
        Assert.Equal(expectedException.Message, response.ErrorMessage);
    }
}
