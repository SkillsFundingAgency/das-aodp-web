using SFA.DAS.Aodp.Domain.Files;
using SFA.DAS.AODP.Application.Queries.Files;
using SFA.DAS.AODP.Infrastructure.File;

namespace SFA.DAS.AODP.Application.Services.Files
{
    public record FileUploadResult(Guid FileId, FileStorageLocation Location);

    /// <summary>
    /// The single entry point for anything file-related — upload, post-upload readback, and
    /// download of a previously-uploaded file. Every method here takes care of both the
    /// FileRecord and the blob together, so a caller can't accidentally touch one without the
    /// other. Raw blob access lives in IBlobStorageService, which only this class depends on.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Creates the FileRecord before the blob exists, then uploads to that exact location.
        /// A Defender scan-result event can then never arrive for a record that isn't there yet.
        /// </summary>
        Task<FileUploadResult> UploadAsync(
            FileCategory category,
            FileContext? context,
            string fileName,
            string? contentType,
            Stream stream,
            string uploadedBy,
            Guid? id = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Polls until the upload's malware scan completes, then opens a read stream to it.
        /// Returns null if it's still not scanned by the time the wait window elapses. For use
        /// immediately after UploadAsync in the same request — not a general download method.
        /// </summary>
        Task<Stream?> GetCleanFileStreamAsync(
            FileUploadResult upload,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Opens a previously-uploaded file for download. A single check, no waiting — by the
        /// time something is being downloaded its scan is expected to have long since finished.
        /// Returns null if the file doesn't exist or isn't scanned clean.
        /// </summary>
        Task<Stream?> DownloadAsync(Guid fileId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Same as DownloadAsync(Guid), but for a caller that already has the file's metadata
        /// (e.g. iterating a list from one GetFileMetadataQuery call) — avoids an extra lookup
        /// per file.
        /// </summary>
        Task<Stream?> DownloadAsync(FileMetadataDto file, CancellationToken cancellationToken = default);

        /// <summary>
        /// Polls until the most recent upload for this category is confirmed scanned clean, for
        /// callers that only need a yes/no before proceeding (e.g. gating a job request) rather
        /// than the file's content. Only meaningful for categories with a single, reused record
        /// per category (see CreateFileMetadataCommandHandler.SingleRecordCategories) — for
        /// anything else this could match the wrong upload.
        /// </summary>
        Task<bool> WaitForCleanFileAsync(FileCategory category, CancellationToken cancellationToken = default);
    }
}
