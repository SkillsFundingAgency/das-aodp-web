//using Microsoft.Extensions.Options;
//using SFA.DAS.AODP.Infrastructure.File;
//using SFA.DAS.AODP.Models.Exceptions.FormValidation;
//using SFA.DAS.AODP.Models.Forms;
//using SFA.DAS.AODP.Models.Settings;
//using SFA.DAS.AODP.Web.Models.Application;

//namespace SFA.DAS.AODP.Web.Validators
//{
//    public class FileValidatorTests : IAnswerValidator
//    {
//        private readonly FormBuilderSettings _formBuilderSettings;
//        private readonly IFileService _fileService;

//        public FileValidatorTests(IOptions<FormBuilderSettings> formBuilderSettings, IFileService fileService)
//        {
//            _formBuilderSettings = formBuilderSettings.Value;
//            _fileService = fileService;
//        }
//        public List<QuestionType> QuestionTypes => [QuestionType.File];

//        public void Validate(GetApplicationPageByIdQueryResponse.Question question, ApplicationPageViewModel.Answer answer, ApplicationPageViewModel model)
//        {
//            var required = question.Required;

//            if (required && (answer == null || answer.FormFiles == null || answer.FormFiles.Count == 0))
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"Please provide the requested files.");
//            if (answer?.FormFiles == null) return;

//            var maxNumberOfFiles = Math.Min(_formBuilderSettings.MaxUploadNumberOfFiles, question.FileUpload.NumberOfFiles ?? _formBuilderSettings.MaxUploadNumberOfFiles);

//            var existingFilesCount = _fileService.ListBlobs($"{model.ApplicationId}/{question.Id}").Count;

//            if (answer.FormFiles.Count + existingFilesCount > maxNumberOfFiles)
//            {
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"You can only upload up to {maxNumberOfFiles} file{(maxNumberOfFiles == 1 ? "" : "s")}.");
//            }

//            var maxSize = Math.Min(_formBuilderSettings.MaxUploadFileSize, question.FileUpload.MaxSize ?? _formBuilderSettings.MaxUploadFileSize);
//            long maxSizeBytes = maxSize * 1024 * 1024;
//            foreach (var file in answer.FormFiles)
//            {
//                if (file.Length > maxSizeBytes)
//                {
//                    throw new QuestionValidationFailedException(question.Id, question.Title, $"You can only upload files up to {maxSize}mb.");
//                }

//                var fileExtension = Path.GetExtension(file.FileName);
//                if (!_formBuilderSettings.UploadFileTypesAllowed.Contains(fileExtension, StringComparer.InvariantCultureIgnoreCase))
//                {
//                    throw new QuestionValidationFailedException(question.Id, question.Title, $"You can only upload files of the following types: {string.Join(",", _formBuilderSettings.UploadFileTypesAllowed)}");
//                }
//            }

//        }
//    }
//}