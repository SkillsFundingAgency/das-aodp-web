
using SFA.DAS.AODP.Web.Models.Import;

namespace SFA.DAS.AODP.Web.Helpers.File
{
    public interface IMessageFileValidationService
    {
        void ValidateFiles(List<IFormFile> files);

        Task<bool> ValidateImportFile(
            IFormFile? file,
            string? fileName,
            string[] headerKeywords,
            ImportFileValidationOptions importFileValidationOptions,
            CancellationToken cancellationToken);
    }
}