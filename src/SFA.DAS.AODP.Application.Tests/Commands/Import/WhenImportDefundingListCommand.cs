using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Moq;
using SFA.DAS.AODP.Application.Commands.Import;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.UnitTests.Commands.Import;

public class WhenImportDefundingListCommand
{
    [Fact]
    public async Task ApiClientReturnsResponse_ShouldReturnsSuccessAndValue()
    {
        // Arrange
        var mockApiClient = new Mock<IApiClient>(MockBehavior.Strict);
        var file = CreateFormFile();
        var command = new ImportDefundingListCommand { File = file };

        var expectedResponse = new ImportDefundingListResponse
        {
            ImportedCount = 42,
            Message = "Imported successfully"
        };

        IPostApiRequest? capturedRequest = null;

        mockApiClient
            .Setup(c => c.PostWithMultipartFormData<ImportDefundingListResponse>(It.IsAny<IPostApiRequest>()))
            .Callback<IPostApiRequest>(req => capturedRequest = req)
            .ReturnsAsync(expectedResponse);

        var handler = new ImportDefundingListCommandHandler(mockApiClient.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(expectedResponse.ImportedCount, result.Value.ImportedCount);
        Assert.Equal(expectedResponse.Message, result.Value.Message);

        Assert.NotNull(capturedRequest);
        Assert.Same(file, capturedRequest!.Data);

        mockApiClient.VerifyAll();
    }

    [Fact]
    public async Task ApiClientThrowsException_ShouldReturnsError()
    {
        // Arrange
        var mockApiClient = new Mock<IApiClient>(MockBehavior.Strict);
        var file = CreateFormFile();
        var command = new ImportDefundingListCommand { File = file };

        var ex = new InvalidOperationException("API failure");

        mockApiClient
            .Setup(c => c.PostWithMultipartFormData<ImportDefundingListResponse>(It.IsAny<IPostApiRequest>()))
            .ThrowsAsync(ex);

        var handler = new ImportDefundingListCommandHandler(mockApiClient.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(ex.Message, result.ErrorMessage);
        Assert.NotNull(result.Value);

        mockApiClient.VerifyAll();
    }

    private static FormFile CreateFormFile(string content = "id,name\n1,Test", string fileName = "test.csv")
    {
        var contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(contentBytes);
        // FormFile(Stream baseStream, long baseStreamOffset, long length, string name, string fileName)
        return new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/csv"
        };
    }
}
