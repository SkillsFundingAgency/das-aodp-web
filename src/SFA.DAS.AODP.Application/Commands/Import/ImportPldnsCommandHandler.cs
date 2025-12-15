using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MediatR;
using SFA.DAS.AODP.Application.Helpers;
using System.Text;

namespace SFA.DAS.AODP.Application.Commands.Import;

public class ImportPldnsCommandHandler : IRequestHandler<ImportPldnsCommand, BaseMediatrResponse<ImportPldnsCommandResponse>>
{
    private const string GenericErrorMessage = "The file you provided does not match the required format for a PLDNS list.";

    public async Task<BaseMediatrResponse<ImportPldnsCommandResponse>> Handle(ImportPldnsCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<ImportPldnsCommandResponse>();

        try
        {
            // Validate request and file type
            var (IsValid, ErrorMessage) = ImportHelper.ValidateRequest(request.File, request.FileName);
            if (!IsValid)
            {
                response.Success = false;
                response.ErrorMessage = ErrorMessage;
                response.Value = new ImportPldnsCommandResponse { ImportedCount = 0 };
                return response;
            }

            await using var ms = new MemoryStream();
            await request.File!.CopyToAsync(ms, cancellationToken);
            ms.Position = 0;

            using var document = SpreadsheetDocument.Open(ms, false);
            var workbookPart = document.WorkbookPart ?? throw new InvalidOperationException("Workbook part missing.");
            var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;

            var sheet = FindSheet(workbookPart, "PLDNS V12F");
            if (sheet == null)
            {
                response.Success = false;
                response.ErrorMessage = GenericErrorMessage;
                response.Value = new ImportPldnsCommandResponse { ImportedCount = 0 };
                return response;
            }

            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
            var sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
            if (sheetData == null)
            {
                response.Success = false;
                response.ErrorMessage = GenericErrorMessage;
                response.Value = new ImportPldnsCommandResponse { ImportedCount = 0 };
                return response;
            }

            var rows = sheetData.Elements<Row>().ToList();
            if (rows.Count <= 1)
            {
                response.Success = false;
                response.ErrorMessage = GenericErrorMessage;
                response.Value = new ImportPldnsCommandResponse { ImportedCount = 0 };
                return response;
            }

            var headerKeywords = new[] {
                "text qan","list updated","note",
                "pldns 14-16","pldns 16-19","pldns local flex",
                "legal entitlement","digital entitlement","esf l3/l4",
                "pldns loans","lifelong learning entitlement","level 3 free courses",
                "pldns cof","start date"
            };

            var (headerRow, headerIndex) = ImportHelper.DetectHeaderRow(rows, sharedStrings, headerKeywords, defaultRowIndex: 1, minMatches: 1);
            if (headerIndex < 0)
            {
                response.Success = false;
                response.ErrorMessage = GenericErrorMessage;
                response.Value = new ImportPldnsCommandResponse { ImportedCount = 0 };
                return response;
            }

            var headerMap = ImportHelper.BuildHeaderMap(headerRow, sharedStrings);
            var columns = MapColumns(headerMap);

            var missingColumns = columns.GetType()
                .GetProperties()
                .Where(p => p.PropertyType == typeof(string))
                .Select(p => new { Name = p.Name, Value = (string?)p.GetValue(columns) })
                .Where(x => string.IsNullOrWhiteSpace(x.Value))
                .Select(x => x.Name)
                .ToList();

            if (missingColumns.Count > 0)
            {
                response.Success = false;
                response.ErrorMessage = GenericErrorMessage;
                response.Value = new ImportPldnsCommandResponse { ImportedCount = 0 };
                return response;
            }

            response.Success = true;
            response.Value = new ImportPldnsCommandResponse { ImportedCount = 0 };
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = ex.Message;
        }

        return response;
    }

    private static Sheet? FindSheet(WorkbookPart workbookPart, string targetSheetName)
    {
        return workbookPart.Workbook.Sheets!
            .Cast<Sheet?>()
            .FirstOrDefault(s => string.Equals((s?.Name!.Value ?? string.Empty).Trim(), targetSheetName, StringComparison.OrdinalIgnoreCase));
    }

    private sealed record ColumnNames(
        string? Qan,
        string? ListUpdated,
        string? Note,
        string? P14To16,
        string? P14To16Note,
        string? P16To19,
        string? P16To19Note,
        string? LocalFlex,
        string? LocalFlexNote,
        string? LegalL2L3,
        string? LegalL2L3Note,
        string? LegalEngMaths,
        string? LegalEngMathsNote,
        string? Digital,
        string? DigitalNote,
        string? Esf,
        string? EsfNote,
        string? Loans,
        string? LoansNote,
        string? Lle,
        string? LleNote,
        string? Fcfj,
        string? FcfjNote,
        string? Cof,
        string? CofNote,
        string? StartDate,
        string? StartDateNote
    );

    private static ColumnNames MapColumns(IDictionary<string, string> headerMap)
    {
        return new ColumnNames(
            ImportHelper.FindColumn(headerMap, "text QAN"),
            ImportHelper.FindColumn(headerMap, "Date PLDNS list updated", "list updated"),
            ImportHelper.FindColumn(headerMap, "NOTE", "Notes"),
            ImportHelper.FindColumn(headerMap, "PLDNS 14-16"),
            ImportHelper.FindColumn(headerMap, "NOTES PLDNS 14-16"),
            ImportHelper.FindColumn(headerMap, "PLDNS 16-19"),
            ImportHelper.FindColumn(headerMap, "NOTES PLDNS 16-19"),
            ImportHelper.FindColumn(headerMap, "PLDNS Local flex"),
            ImportHelper.FindColumn(headerMap, "NOTES PLDNS Local flex"),
            ImportHelper.FindColumn(headerMap, "PLDNS Legal entitlement L2/L3"),
            ImportHelper.FindColumn(headerMap, "NOTES PLDNS Legal entitlement L2/L3"),
            ImportHelper.FindColumn(headerMap, "PLDNS Legal entitlement Eng/Maths"),
            ImportHelper.FindColumn(headerMap, "NOTES PLDNS Legal entitlement Eng/Maths"),
            ImportHelper.FindColumn(headerMap, "PLDNS Digital entitlement"),
            ImportHelper.FindColumn(headerMap, "NOTES PLDNS Digital entitlement"),
            ImportHelper.FindColumn(headerMap, "PLDNS ESF L3/L4"),
            ImportHelper.FindColumn(headerMap, "NOTES PLDNS ESF L3/L4"),
            ImportHelper.FindColumn(headerMap, "PLDNS Loans"),
            ImportHelper.FindColumn(headerMap, "NOTES PLDNS Loans"),
            ImportHelper.FindColumn(headerMap, "PLDNS Lifelong Learning Entitlement"),
            ImportHelper.FindColumn(headerMap, "NOTES PLDNS Lifelong Learning Entitlement"),
            ImportHelper.FindColumn(headerMap, "PLDNS Level 3 Free Courses for Jobs"),
            ImportHelper.FindColumn(headerMap, "Level 3 Free Courses for Jobs (Previously known as National skills fund L3 extended entitlement)"),
            ImportHelper.FindColumn(headerMap, "PLDNS CoF"),
            ImportHelper.FindColumn(headerMap, "NOTES  PLDNS CoF"),
            ImportHelper.FindColumn(headerMap, "Start date"),
            ImportHelper.FindColumn(headerMap, "NOTES Start date")
        );
    }
}
