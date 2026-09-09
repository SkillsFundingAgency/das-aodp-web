using MediatR;
using Moq;
using SFA.DAS.Aodp.Domain.Files;
using SFA.DAS.AODP.Application.Commands.Files;
using SFA.DAS.AODP.Application.Queries.Files;
using SFA.DAS.AODP.Application.Queries.Files.Get;
using SFA.DAS.AODP.Application.Services.Files;
using SFA.DAS.AODP.Infrastructure.File;
using Xunit;

namespace SFA.DAS.AODP.Application.Tests.Services.Files;

public class FileServiceTests
{
    private readonly Mock<IFileStorageLocationPolicy> _locationPolicy = new();
    private readonly Mock<IBlobStorageService> _blobStorageService = new();
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IDelayService> _delayService = new();
    private readonly FileService _service;

    private static readonly FileStorageLocation Location = new("container", "path/file.pdf");
    private const string UploadedBy = "test-user";

    public FileServiceTests()
    {
        _service = new FileService(
            _locationPolicy.Object,
            _blobStorageService.Object,
            _mediator.Object,
            _delayService.Object);

        _delayService
            .Setup(d => d.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static BaseMediatrResponse<GetFileMetadataQueryResponse> MetadataResponse(params FileMetadataDto[] files) =>
        new()
        {
            Success = true,
            Value = new GetFileMetadataQueryResponse { Files = files.ToList() }
        };

    private static FileMetadataDto File(Guid fileId, bool isDownloadable) => new()
    {
        FileId = fileId,
        FileName = "file.pdf",
        BlobContainer = Location.Container,
        BlobPath = Location.BlobPath,
        IsDownloadable = isDownloadable
    };

    [Fact]
    public async Task UploadAsync_CreatesRecordThenUploadsBlob_ReturnsResult()
    {
        _locationPolicy
            .Setup(p => p.Resolve(FileCategory.Pldns, null))
            .Returns(Location);

        _mediator
            .Setup(m => m.Send(It.IsAny<CreateFileMetadataCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<EmptyResponse> { Success = true });

        using var stream = new MemoryStream();

        var result = await _service.UploadAsync(
            FileCategory.Pldns, null, "file.pdf", "application/pdf", stream, UploadedBy);

        Assert.Multiple(() =>
        {
            Assert.Equal(Location, result.Location);
            Assert.NotEqual(Guid.Empty, result.FileId);

            _mediator.Verify(m => m.Send(
                It.Is<CreateFileMetadataCommand>(c =>
                    c.Id == result.FileId &&
                    c.FileCategory == FileCategory.Pldns &&
                    c.FileName == "file.pdf" &&
                    c.BlobContainer == Location.Container &&
                    c.BlobPath == Location.BlobPath &&
                    c.UploadedBy == UploadedBy),
                It.IsAny<CancellationToken>()),
                Times.Once);

            _blobStorageService.Verify(b => b.UploadAsync(
                Location, "file.pdf", "application/pdf", stream),
                Times.Once);
        });
    }

    [Fact]
    public async Task UploadAsync_UsesProvidedId_WhenSpecified()
    {
        var suppliedId = Guid.NewGuid();

        _locationPolicy
            .Setup(p => p.Resolve(FileCategory.Pldns, null))
            .Returns(Location);

        _mediator
            .Setup(m => m.Send(It.IsAny<CreateFileMetadataCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<EmptyResponse> { Success = true });

        using var stream = new MemoryStream();

        var result = await _service.UploadAsync(
            FileCategory.Pldns, null, "file.pdf", "application/pdf", stream, UploadedBy, suppliedId);

        Assert.Equal(suppliedId, result.FileId);
    }

    [Fact]
    public async Task UploadAsync_WhenMetadataCreationFails_ThrowsAndDoesNotUploadBlob()
    {
        _locationPolicy
            .Setup(p => p.Resolve(FileCategory.Pldns, null))
            .Returns(Location);

        _mediator
            .Setup(m => m.Send(It.IsAny<CreateFileMetadataCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<EmptyResponse> { Success = false, ErrorMessage = "db error" });

        using var stream = new MemoryStream();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UploadAsync(FileCategory.Pldns, null, "file.pdf", "application/pdf", stream, UploadedBy));

        _blobStorageService.Verify(b => b.UploadAsync(
            It.IsAny<FileStorageLocation>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCleanFileStreamAsync_AlreadyDownloadable_ReturnsStreamImmediately()
    {
        var fileId = Guid.NewGuid();
        var upload = new FileUploadResult(fileId, Location);

        _mediator
            .Setup(m => m.Send(It.IsAny<GetFileMetadataQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MetadataResponse(File(fileId, isDownloadable: true)));

        using var expectedStream = new MemoryStream();
        _blobStorageService
            .Setup(b => b.OpenReadStreamAsync(Location.Container, Location.BlobPath))
            .ReturnsAsync(expectedStream);

        var result = await _service.GetCleanFileStreamAsync(upload);

        Assert.Same(expectedStream, result);
        _delayService.Verify(d => d.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetCleanFileStreamAsync_BecomesDownloadableAfterRetry_ReturnsStream()
    {
        var fileId = Guid.NewGuid();
        var upload = new FileUploadResult(fileId, Location);

        _mediator
            .SetupSequence(m => m.Send(It.IsAny<GetFileMetadataQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MetadataResponse(File(fileId, isDownloadable: false)))
            .ReturnsAsync(MetadataResponse(File(fileId, isDownloadable: true)));

        using var expectedStream = new MemoryStream();
        _blobStorageService
            .Setup(b => b.OpenReadStreamAsync(Location.Container, Location.BlobPath))
            .ReturnsAsync(expectedStream);

        var result = await _service.GetCleanFileStreamAsync(upload);

        Assert.Same(expectedStream, result);
        _delayService.Verify(d => d.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCleanFileStreamAsync_NeverBecomesDownloadable_ReturnsNullAfterAllRetries()
    {
        var fileId = Guid.NewGuid();
        var upload = new FileUploadResult(fileId, Location);

        _mediator
            .Setup(m => m.Send(It.IsAny<GetFileMetadataQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MetadataResponse(File(fileId, isDownloadable: false)));

        var result = await _service.GetCleanFileStreamAsync(upload);

        Assert.Null(result);
        _blobStorageService.Verify(b => b.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DownloadAsync_ByFileId_WhenDownloadable_ReturnsStream()
    {
        var fileId = Guid.NewGuid();

        _mediator
            .Setup(m => m.Send(It.IsAny<GetFileMetadataQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MetadataResponse(File(fileId, isDownloadable: true)));

        using var expectedStream = new MemoryStream();
        _blobStorageService
            .Setup(b => b.OpenReadStreamAsync(Location.Container, Location.BlobPath))
            .ReturnsAsync(expectedStream);

        var result = await _service.DownloadAsync(fileId);

        Assert.Same(expectedStream, result);
    }

    [Fact]
    public async Task DownloadAsync_ByFileId_WhenFileNotFound_ReturnsNull()
    {
        var fileId = Guid.NewGuid();

        _mediator
            .Setup(m => m.Send(It.IsAny<GetFileMetadataQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MetadataResponse());

        var result = await _service.DownloadAsync(fileId);

        Assert.Null(result);
    }

    [Fact]
    public async Task DownloadAsync_ByDto_WhenNotDownloadable_ReturnsNull()
    {
        var file = File(Guid.NewGuid(), isDownloadable: false);

        var result = await _service.DownloadAsync(file);

        Assert.Null(result);
        _blobStorageService.Verify(b => b.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task WaitForCleanFileAsync_AlreadyClean_ReturnsTrueImmediately()
    {
        _mediator
            .Setup(m => m.Send(It.IsAny<GetFileMetadataQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MetadataResponse(File(Guid.NewGuid(), isDownloadable: true)));

        var result = await _service.WaitForCleanFileAsync(FileCategory.Pldns);

        Assert.True(result);
        _delayService.Verify(d => d.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WaitForCleanFileAsync_BecomesCleanAfterRetry_ReturnsTrue()
    {
        _mediator
            .SetupSequence(m => m.Send(It.IsAny<GetFileMetadataQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MetadataResponse(File(Guid.NewGuid(), isDownloadable: false)))
            .ReturnsAsync(MetadataResponse(File(Guid.NewGuid(), isDownloadable: true)));

        var result = await _service.WaitForCleanFileAsync(FileCategory.Pldns);

        Assert.True(result);
        _delayService.Verify(d => d.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WaitForCleanFileAsync_NeverBecomesClean_ReturnsFalseAfterAllRetries()
    {
        _mediator
            .Setup(m => m.Send(It.IsAny<GetFileMetadataQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MetadataResponse());

        var result = await _service.WaitForCleanFileAsync(FileCategory.Pldns);

        Assert.False(result);
        _delayService.Verify(d => d.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
    }
}
