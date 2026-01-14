using DocumentFormat.OpenXml.Packaging;
using SFA.DAS.AODP.Application.Helpers;
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

        public async Task<bool> ValidateImportFile(IFormFile? file, string? fileName, string[] headerKeywords, string targetSheetName, int defaultRowIndex, int minMatches, Func<IDictionary<string, string>, object> mapColumns, CancellationToken cancellationToken)
        {
            // Validate request and file type
            var (IsValid, ErrorMessage) = ImportHelper.ValidateRequest(file, fileName);
            if (!IsValid)
            {
                return false;
            }

            await using var ms = new MemoryStream();
            await file!.CopyToAsync(ms, cancellationToken);
            ms.Position = 0;

            using var document = SpreadsheetDocument.Open(ms, false);
            var workbookPart = document.WorkbookPart ?? throw new InvalidOperationException("Workbook part missing.");
            var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;

            var sheet = ImportHelper.FindSheet(workbookPart, targetSheetName);
            if (sheet == null)
            {
                return false;
            }

            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
            var rows = ImportHelper.GetRowsFromWorksheet(worksheetPart).ToList();
            if (rows.Count <= 1)
            {
                return false;
            }

            var (headerRow, headerIndex) = ImportHelper.DetectHeaderRow(rows, sharedStrings, headerKeywords, defaultRowIndex: defaultRowIndex, minMatches: minMatches);
            if (headerIndex < 0)
            {
                return false;
            }

            var headerMap = ImportHelper.BuildHeaderMap(headerRow, sharedStrings);
            var columns = mapColumns(headerMap);

            var missingColumns = columns.GetType()
                .GetProperties()
                .Where(p => p.PropertyType == typeof(string))
                .Select(p => new { Name = p.Name, Value = (string?)p.GetValue(columns) })
                .Where(x => string.IsNullOrWhiteSpace(x.Value))
                .Select(x => x.Name)
                .ToList();

            if (missingColumns.Count > 0)
            {
                return false;
            }

            return true;
        }
    }
}
