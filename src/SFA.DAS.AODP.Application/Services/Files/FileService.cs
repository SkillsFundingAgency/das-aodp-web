using SFA.DAS.Aodp.Domain.Files;
using SFA.DAS.AODP.Application.Commands.Files;
using SFA.DAS.AODP.Application.Queries.Files;
using SFA.DAS.AODP.Application.Queries.Files.Get;
using SFA.DAS.AODP.Infrastructure.File;

namespace SFA.DAS.AODP.Application.Services.Files
{
    public class FileService : IFileService
    {
        private static readonly TimeSpan[] ScanCheckDelays =
        [
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(4)
        ];

        private readonly IFileStorageLocationPolicy _fileStorageLocationPolicy;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IMediator _mediator;
        private readonly IDelayService _delayService;

        public FileService(
            IFileStorageLocationPolicy fileStorageLocationPolicy,
            IBlobStorageService blobStorageService,
            IMediator mediator,
            IDelayService delayService)
        {
            _fileStorageLocationPolicy = fileStorageLocationPolicy;
            _blobStorageService = blobStorageService;
            _mediator = mediator;
            _delayService = delayService;
        }

        public async Task<FileUploadResult> UploadAsync(
            FileCategory category,
            FileContext? context,
            string fileName,
            string? contentType,
            Stream stream,
            string uploadedBy,
            Guid? id = null,
            CancellationToken cancellationToken = default)
        {
            var location = _fileStorageLocationPolicy.Resolve(category, context);
            var fileId = id ?? Guid.NewGuid();

            var createResult = await _mediator.Send(new CreateFileMetadataCommand
            {
                Id = fileId,
                FileCategory = category,
                ApplicationId = context?.ApplicationId,
                QuestionId = context?.QuestionId,
                MessageId = context?.MessageId,
                FileName = fileName,
                ContentType = contentType ?? string.Empty,
                BlobContainer = location.Container,
                BlobPath = location.BlobPath,
                UploadedBy = uploadedBy,
            }, cancellationToken);

            if (!createResult.Success)
            {
                throw new InvalidOperationException(
                    $"Failed to create file metadata record for {location.Container}/{location.BlobPath}: {createResult.ErrorMessage}");
            }

            await _blobStorageService.UploadAsync(location, fileName, contentType, stream);

            return new FileUploadResult(fileId, location);
        }

        public async Task<Stream?> GetCleanFileStreamAsync(
            FileUploadResult upload,
            CancellationToken cancellationToken = default)
        {
            if (await IsFileDownloadableAsync(upload.FileId, cancellationToken))
            {
                return await _blobStorageService.OpenReadStreamAsync(upload.Location.Container, upload.Location.BlobPath);
            }

            foreach (var delay in ScanCheckDelays)
            {
                await _delayService.DelayAsync(delay, cancellationToken);

                if (await IsFileDownloadableAsync(upload.FileId, cancellationToken))
                {
                    return await _blobStorageService.OpenReadStreamAsync(upload.Location.Container, upload.Location.BlobPath);
                }
            }

            return null;
        }

        public async Task<Stream?> DownloadAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            var response = await _mediator.Send(new GetFileMetadataQuery { FileId = fileId }, cancellationToken);
            var file = response.Success ? response.Value.Files.SingleOrDefault(f => f.FileId == fileId) : null;

            return file is null ? null : await DownloadAsync(file, cancellationToken);
        }

        public async Task<Stream?> DownloadAsync(FileMetadataDto file, CancellationToken cancellationToken = default)
        {
            if (!file.IsDownloadable)
            {
                return null;
            }

            return await _blobStorageService.OpenReadStreamAsync(file.BlobContainer, file.BlobPath);
        }

        public async Task<bool> WaitForCleanFileAsync(FileCategory category, CancellationToken cancellationToken = default)
        {
            if (await IsCategoryFileDownloadableAsync(category, cancellationToken))
            {
                return true;
            }

            foreach (var delay in ScanCheckDelays)
            {
                await _delayService.DelayAsync(delay, cancellationToken);

                if (await IsCategoryFileDownloadableAsync(category, cancellationToken))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> IsFileDownloadableAsync(Guid fileId, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetFileMetadataQuery { FileId = fileId }, cancellationToken);

            return response.Success &&
                   response.Value.Files.Any(f => f.FileId == fileId && f.IsDownloadable);
        }

        private async Task<bool> IsCategoryFileDownloadableAsync(FileCategory category, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(
                new GetFileMetadataQuery { FileCategories = [category] },
                cancellationToken);

            return response.Success &&
                   response.Value.Files.Any(f => f.IsDownloadable);
        }
    }
}
