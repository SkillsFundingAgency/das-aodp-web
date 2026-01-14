
namespace SFA.DAS.AODP.Web.Helpers.File
{
    public interface IMessageFileValidationService
    {
        void ValidateFiles(List<IFormFile> files);

        Task<bool> ValidateImportFile(
            IFormFile? file,
            string? fileName,
            string[] headerKeywords,
            string targetSheetName,
            int defaultRowIndex,
            int minMatches,
            Func<IDictionary<string, string>, object> mapColumns,
            CancellationToken cancellationToken);
    }
}