using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MediatR;
using SFA.DAS.AODP.Application.Helpers;
using System.Text;

namespace SFA.DAS.AODP.Application.Commands.Import;

public class ImportPldnsCommandHandler : IRequestHandler<ImportPldnsCommand, BaseMediatrResponse<ImportPldnsCommandResponse>>
{
    private const int BatchSize = 3000;
    private const string GenericErrorMessage = "The file you provided does not match the required format for a PLDNS list.";

    public async Task<BaseMediatrResponse<ImportPldnsCommandResponse>> Handle(ImportPldnsCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<ImportPldnsCommandResponse>();

        try
        {
            // Validate request and file type
            var (IsValid, Success, ErrorMessage) = ValidateRequest(request);
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

            int headerIndex = FindHeaderIndex(rows, sharedStrings);
            if (headerIndex < 0)
            {
                response.Success = false;
                response.ErrorMessage = GenericErrorMessage;
                response.Value = new ImportPldnsCommandResponse { ImportedCount = 0 };
                return response;
            }
            var headerRow = rows[headerIndex];

            var headerMap = BuildHeaderMap(headerRow, sharedStrings);
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

    private static (bool IsValid, bool Success, string? ErrorMessage) ValidateRequest(ImportPldnsCommand request)
    {
        if (request.File == null || request.File.Length == 0)
            return (false, true, null);

        if (string.IsNullOrWhiteSpace(request.FileName) || !request.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return (false, false, "Unsupported file type. Only .xlsx files are accepted.");

        return (true, false, null);
    }

    private static Sheet? FindSheet(WorkbookPart workbookPart, string targetSheetName)
    {
        return workbookPart.Workbook.Sheets!
            .Cast<Sheet?>()
            .FirstOrDefault(s => string.Equals((s?.Name!.Value ?? string.Empty).Trim(), targetSheetName, StringComparison.OrdinalIgnoreCase));
    }

    private static int FindHeaderIndex(List<Row> rows, SharedStringTable? sharedStrings)
    {
        var headerRow = rows.Count > 1 ? rows[1] : rows[0];
        int headerListIndex = rows.IndexOf(headerRow);

        var headerKeywords = new[] {
                "text qan","list updated","note",
                "pldns 14-16","pldns 16-19","pldns local flex",
                "legal entitlement","digital entitlement","esf l3/l4",
                "pldns loans","lifelong learning entitlement","level 3 free courses",
                "pldns cof","start date"
            };

        for (int r = 0; r < Math.Min(rows.Count, 12); r++)
        {
            var cellTexts = rows[r].Elements<Cell>()
                .Select(c => ImportHelper.GetCellText(c, sharedStrings).Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.ToLowerInvariant())
                .ToList();

            if (cellTexts.Count == 0) continue;

            var matches = cellTexts.Count(ct => headerKeywords.Any(k => ct.Contains(k)));
            if (matches >= 1)
            {
                headerListIndex = r;
                break;
            }
        }

        return headerListIndex;
    }

    private static Dictionary<string, string> BuildHeaderMap(Row headerRow, SharedStringTable? sharedStrings)
    {
        var headerMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in headerRow.Elements<Cell>())
        {
            var col = GetColumnName(cell.CellReference?.Value);
            var txt = ImportHelper.GetCellText(cell, sharedStrings);
            if (!string.IsNullOrWhiteSpace(col) && !string.IsNullOrWhiteSpace(txt))
                headerMap[col!] = txt.Trim();
        }
        return headerMap;
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

    //private static string? GetValue(Dictionary<string, string> map, string? column)
    //{
    //    if (string.IsNullOrWhiteSpace(column))
    //    {
    //        return null;
    //    }

    //    if (map.TryGetValue(column!, out var v))
    //    {
    //        return v;
    //    }

    //    return null;
    //}

    private static string? GetColumnName(string? cellReference)
    {
        if (string.IsNullOrWhiteSpace(cellReference)) return null;
        var sb = new StringBuilder();
        foreach (var ch in cellReference)
        {
            if (char.IsLetter(ch)) sb.Append(ch);
            else break;
        }
        return sb.ToString();
    }

    //private static DateTime? TryParseDate(string? txt, CultureInfo culture, string[] formats)
    //{
    //    if (string.IsNullOrWhiteSpace(txt)) return null;
    //    txt = txt!.Trim();
    //    if (DateTime.TryParse(txt, culture, DateTimeStyles.None, out var dt)) return dt.Date;
    //    if (DateTime.TryParseExact(txt, formats, culture, DateTimeStyles.None, out dt)) return dt.Date;
    //    if (double.TryParse(txt, NumberStyles.Any, CultureInfo.InvariantCulture, out var oa))
    //    {
    //        return DateTime.FromOADate(oa);
    //    }
    //    return null;
    //}

    //private static void PopulateCellMap(IEnumerable<Cell> rowCells, SharedStringTable? sharedStrings, Dictionary<string, string> cellMap)
    //{
    //    foreach (var cell in rowCells)
    //    {
    //        var col = GetColumnName(cell.CellReference?.Value);
    //        if (string.IsNullOrWhiteSpace(col)) continue;
    //        var text = ImportHelper.GetCellText(cell, sharedStrings)?.Trim() ?? string.Empty;
    //        if (!cellMap.ContainsKey(col)) cellMap[col] = text;
    //    }
    //}
}
