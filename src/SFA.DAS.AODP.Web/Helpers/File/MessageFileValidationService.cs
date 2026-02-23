using DocumentFormat.OpenXml.Packaging;
using SFA.DAS.AODP.Application.Helpers;
using SFA.DAS.AODP.Models.Common;
using SFA.DAS.AODP.Models.Exceptions;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Models.Import;

namespace SFA.DAS.AODP.Web.Helpers.File
{
    public class MessageFileValidationService : IMessageFileValidationService
    {
        private readonly FormBuilderSettings _formBuilderSettings;

        public MessageFileValidationService(FormBuilderSettings formBuilderSettings)
        {
            _formBuilderSettings = formBuilderSettings;
        }

        public void ValidateFiles(List<IFormFile>? files)
        {
            if (files is null || files.Count == 0)
                return;

            if (files.Any(f => f is null))
                throw new FileUploadPolicyException(FileUploadRejectionReason.MissingFile);

            if (files.Any(f => f.Length == 0))
                throw new FileUploadPolicyException(FileUploadRejectionReason.EmptyFile);

            if (files.Count > _formBuilderSettings.MaxUploadNumberOfFiles)
                throw new FileUploadPolicyException(FileUploadRejectionReason.TooManyFiles);

            long maxAllowedBytes = _formBuilderSettings.MaxUploadFileSize * 1024 * 1024;
            if (files.Any(f => f.Length > maxAllowedBytes))
                throw new FileUploadPolicyException(FileUploadRejectionReason.FileTooLarge);

            if (_formBuilderSettings.UploadFileTypesAllowed is { Count: > 0 })
            {
                foreach (var file in files)
                {
                    var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                    var allowed = _formBuilderSettings.UploadFileTypesAllowed
                        .Select(e => e.ToLowerInvariant());

                    if (!allowed.Contains(ext))
                        throw new FileUploadPolicyException(FileUploadRejectionReason.FileTypeNotAllowed);
                }
            }
        }

        public async Task<bool> ValidateImportFile(IFormFile? file, string? fileName, string[] headerKeywords, ImportFileValidationOptions importFileValidationOptions, CancellationToken cancellationToken)
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

            var sheet = ImportHelper.FindSheet(workbookPart, importFileValidationOptions.TargetSheetName);
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

            var (headerRow, headerIndex) = ImportHelper.DetectHeaderRow(rows, sharedStrings, headerKeywords, defaultRowIndex: importFileValidationOptions.DefaultRowIndex, minMatches: importFileValidationOptions.MinMatches);
            if (headerIndex < 0)
            {
                return false;
            }

            var headerMap = ImportHelper.BuildHeaderMap(headerRow, sharedStrings);
            var columns = importFileValidationOptions.MapColumns(headerMap);

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
