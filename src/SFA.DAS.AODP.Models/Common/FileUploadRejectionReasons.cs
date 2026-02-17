namespace SFA.DAS.AODP.Models.Common
{
    public enum FileUploadRejectionReason
    {
        MissingFile,
        MissingFileName,
        InvalidFileName,
        MissingExtension,
        FileTypeNotAllowed,
        UnknownFileSize,
        EmptyFile,
        FileTooLarge,
        TooManyFiles
    }
}
