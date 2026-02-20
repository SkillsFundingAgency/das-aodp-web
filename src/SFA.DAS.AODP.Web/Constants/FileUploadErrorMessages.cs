namespace SFA.DAS.AODP.Web.Constants
{
    public static class FileUploadErrorMessages
    {
        public const string FileDoesNotExist = "File does not exist.";
        public const string MissingFileName = "Select a file to upload.";
        public const string InvalidFileName = "The file name is not valid.";
        public const string MissingExtension = "The file must have an extension, for example .pdf or .docx.";
        public const string FileTypeNotAllowed = "File type is not included in the allowed file types: {0}.";
        public const string UnknownFileSize = "Unable to determine file size.";
        public const string EmptyFile = "The selected file is empty.";
        public const string FileTooLarge = "File size exceeds max allowed size of {0}mb.";
        public const string TooManyFiles = "Cannot upload more than {0} files";
        public const string Default = "The file could not be uploaded.";
    }
}
