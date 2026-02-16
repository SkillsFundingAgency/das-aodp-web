using SFA.DAS.AODP.Models.Common;
using SFA.DAS.AODP.Web.Constants;

namespace SFA.DAS.AODP.Web.Extensions
{
    public static class FileUploadRejectionReasonExtensions
    {
        public static string ToUserMessage(this FileUploadRejectionReason reason) => reason switch
        {
            FileUploadRejectionReason.MissingFile => FileUploadErrorMessages.FileDoesNotExist,
            FileUploadRejectionReason.MissingFileName => FileUploadErrorMessages.MissingFileName,
            FileUploadRejectionReason.InvalidFileName => FileUploadErrorMessages.InvalidFileName,
            FileUploadRejectionReason.MissingExtension => FileUploadErrorMessages.MissingExtension,
            FileUploadRejectionReason.FileTypeNotAllowed => FileUploadErrorMessages.FileTypeNotAllowed,
            FileUploadRejectionReason.UnknownFileSize => FileUploadErrorMessages.UnknownFileSize,
            FileUploadRejectionReason.EmptyFile => FileUploadErrorMessages.EmptyFile,
            FileUploadRejectionReason.FileTooLarge => FileUploadErrorMessages.FileTooLarge,
            FileUploadRejectionReason.TooManyFiles => FileUploadErrorMessages.TooManyFiles,
            _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
        };
    }
}
