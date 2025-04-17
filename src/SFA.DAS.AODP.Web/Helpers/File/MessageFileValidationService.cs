using SFA.DAS.AODP.Models.Settings;

namespace SFA.DAS.AODP.Web.Helpers.File
{
    public class MessageFileValidationService : IMessageFileValidationService
    {
        private readonly FormBuilderSettings _formBuilderSettings;

        public MessageFileValidationService(FormBuilderSettings formBuilderSettings)
        {
            _formBuilderSettings = formBuilderSettings;
        }

        public void ValidateFiles(List<IFormFile> files)
        {
            if(files.Count > _formBuilderSettings.MaxUploadNumberOfFiles)
            {
                throw new Exception($"Cannot upload more than {_formBuilderSettings.MaxUploadNumberOfFiles} files");
            }
            var maxSize = _formBuilderSettings.MaxUploadFileSize;
            long maxSizeBytes = maxSize * 1024 * 1024;
            foreach (var file in files)
            {
                if (file.Length > maxSizeBytes)
                {
                    throw new Exception($"File size exceeds max allowed size of {maxSize}mb.");
                }

                var fileExtension = Path.GetExtension(file.FileName);
                if (!_formBuilderSettings.UploadFileTypesAllowed.Contains(fileExtension, StringComparer.InvariantCultureIgnoreCase))
                {
                    throw new Exception($"File type is not included in the allowed file types: {string.Join(",", _formBuilderSettings.UploadFileTypesAllowed)}");
                }
            }
        }
    }
}
