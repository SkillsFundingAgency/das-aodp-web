using SFA.DAS.AODP.Models.Common;
using SFA.DAS.AODP.Models.Exceptions;
using SFA.DAS.AODP.Models.Settings;
using System.Text;

namespace SFA.DAS.AODP.Infrastructure.Common.IO;
public sealed class FileUploadValidator
{
    private readonly FormBuilderSettings _settings;
    public FileUploadValidator(FormBuilderSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public void ValidateOrThrow(string? originalFileName, Stream? stream)
    {
        if (stream is null)
            throw new FileUploadPolicyException(FileUploadRejectionReason.MissingFile);

        var safeFileName = (Path.GetFileName(originalFileName ?? string.Empty) ?? string.Empty).Trim();
        safeFileName = safeFileName.Normalize(NormalizationForm.FormC);

        if (string.IsNullOrWhiteSpace(safeFileName))
            throw new FileUploadPolicyException(FileUploadRejectionReason.MissingFileName);

        if (safeFileName.Length <= 1 || safeFileName.StartsWith(".", StringComparison.Ordinal))
            throw new FileUploadPolicyException(FileUploadRejectionReason.InvalidFileName);

        if (safeFileName.Length > WindowsFileNameRules.MaxFileNameLength)
            throw new FileUploadPolicyException(FileUploadRejectionReason.InvalidFileName);

        if (safeFileName.EndsWith(".", StringComparison.Ordinal) || safeFileName.EndsWith(" ", StringComparison.Ordinal))
            throw new FileUploadPolicyException(FileUploadRejectionReason.InvalidFileName);

        if (safeFileName.Any(char.IsControl))
            throw new FileUploadPolicyException(FileUploadRejectionReason.InvalidFileName);

        var baseName = Path.GetFileNameWithoutExtension(safeFileName);
        if (WindowsFileNameRules.ReservedBaseNames.Contains(baseName))
            throw new FileUploadPolicyException(FileUploadRejectionReason.InvalidFileName);

        var ext = (Path.GetExtension(safeFileName) ?? string.Empty).Trim().ToLowerInvariant();

        var allowed = (_settings.UploadFileTypesAllowed ?? new List<string>())
            .Select(x => (x ?? string.Empty).Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.StartsWith('.') ? x.ToLowerInvariant() : "." + x.ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (allowed.Count > 0)
        {
            if (string.IsNullOrWhiteSpace(ext))
                throw new FileUploadPolicyException(FileUploadRejectionReason.MissingExtension);

            if (!allowed.Contains(ext))
                throw new FileUploadPolicyException(FileUploadRejectionReason.FileTypeNotAllowed);
        }

        var configuredMb = _settings.MaxUploadFileSize;
        if (configuredMb <= 0)
            throw new FileUploadPolicyException(FileUploadRejectionReason.FileTooLarge);

        var maxBytes = (long)configuredMb * 1024L * 1024L;

        if (!stream.CanSeek)
            throw new FileUploadPolicyException(FileUploadRejectionReason.UnknownFileSize);

        if (stream.Length <= 0)
            throw new FileUploadPolicyException(FileUploadRejectionReason.EmptyFile);

        if (stream.Length > maxBytes)
            throw new FileUploadPolicyException(FileUploadRejectionReason.FileTooLarge);

        if (stream.Position != 0)
            stream.Position = 0;
    }
}
