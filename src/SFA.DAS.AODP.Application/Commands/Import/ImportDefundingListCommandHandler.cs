using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MediatR;
using SFA.DAS.AODP.Application.Helpers;
using System.Text;

namespace SFA.DAS.AODP.Application.Commands.Import;

public class ImportDefundingListCommandHandler : IRequestHandler<ImportDefundingListCommand, BaseMediatrResponse<ImportDefundingListCommandResponse>>
{
    private const string GenericErrorMessage = "The selected file must use the correct format";
    public async Task<BaseMediatrResponse<ImportDefundingListCommandResponse>> Handle(ImportDefundingListCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<ImportDefundingListCommandResponse>();

        try
        {
            // Validate request and file type
            var (IsValid, Success, ErrorMessage) = ValidateRequest(request);
            if (!IsValid)
            {
                response.Success = false;
                response.ErrorMessage = ErrorMessage;
                response.Value = new ImportDefundingListCommandResponse { ImportedCount = 0 };
                return response;
            }

            // Load file into memory
            await using var ms = new MemoryStream();
            await request.File!.CopyToAsync(ms, cancellationToken);
            ms.Position = 0;

            using var document = SpreadsheetDocument.Open(ms, false);
            var workbookPart = document.WorkbookPart ?? throw new InvalidOperationException("Workbook part missing.");
            var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;

            // Get target sheet
            var targetSheetName = "Approval not extended";
            var chosenSheet = workbookPart.Workbook.Sheets!
                .Cast<Sheet?>()
                .FirstOrDefault(s => string.Equals((s?.Name!.Value ?? string.Empty).Trim(), targetSheetName, StringComparison.OrdinalIgnoreCase));

            if (chosenSheet == null)
            {
                response.Success = false;
                response.ErrorMessage = GenericErrorMessage;
                response.Value = new ImportDefundingListCommandResponse { ImportedCount = 0 };
                return response;
            }

            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(chosenSheet.Id!);
            var rows = GetRowsFromWorksheet(worksheetPart).ToList();
            if (rows.Count <= 1)
            {
                response.Success = false;
                response.ErrorMessage = GenericErrorMessage;
                response.Value = new ImportDefundingListCommandResponse { ImportedCount = 0 };
                return response;
            }

            // Detect header row
            var (headerRow, headerIndex) = DetectHeaderRow(rows, sharedStrings);

            if (headerIndex < 0)
            {
                response.Success = false;
                response.ErrorMessage = GenericErrorMessage;
                response.Value = new ImportDefundingListCommandResponse { ImportedCount = 0 };
                return response;
            }

            // Build header map
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
                response.Value = new ImportDefundingListCommandResponse { ImportedCount = 0 };
                return response;
            }

            response.Success = true;
            response.Value = new ImportDefundingListCommandResponse { ImportedCount = 0 };
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = ex.Message;
        }

        return response;
    }

    private static (bool IsValid, bool Success, string? ErrorMessage) ValidateRequest(ImportDefundingListCommand request)
    {
        if (request.File == null || request.File.Length == 0)
            return (false, true, null);

        if (string.IsNullOrWhiteSpace(request.FileName) || !request.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return (false, false, "Unsupported file type. Only .xlsx files are accepted.");

        return (true, false, null);
    }

    private static IEnumerable<Row> GetRowsFromWorksheet(WorksheetPart worksheetPart)
    {
        var sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
        if (sheetData == null) yield break;
        foreach (var row in sheetData.Elements<Row>()) yield return row;
    }

    private static (Row headerRow, int headerIndex) DetectHeaderRow(List<Row> rows, SharedStringTable? sharedStrings)
    {
        Row headerRow = rows.Count > 6 ? rows[6] : rows[0];
        int headerListIndex = rows.IndexOf(headerRow);

        var headerKeywords = new[] { "qualification", "qan", "title", "award", "guided", "sector", "route", "funding", "in scope", "comments" };

        for (int r = 0; r < Math.Min(rows.Count, 12); r++)
        {
            var cellTexts = rows[r].Elements<Cell>()
                .Select(c => ImportHelper.GetCellText(c, sharedStrings).Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.ToLowerInvariant())
                .ToList();

            if (cellTexts.Count == 0) continue;

            var matches = cellTexts.Count(ct => headerKeywords.Any(k => ct.Contains(k)));
            if (matches >= 2)
            {
                headerRow = rows[r];
                headerListIndex = r;
                break;
            }
        }

        return (headerRow, headerListIndex);
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

    private static ColumnNames MapColumns(IDictionary<string, string> headerMap)
    {
        return new ColumnNames(
            ImportHelper.FindColumn(headerMap, "Qualification number"),
            ImportHelper.FindColumn(headerMap, "Title"),
            ImportHelper.FindColumn(headerMap, "Awarding organisation"),
            ImportHelper.FindColumn(headerMap, "Guided Learning Hours"),
            ImportHelper.FindColumn(headerMap, "Sector Subject Area Tier 2"),
            ImportHelper.FindColumn(headerMap, "Relevant route"),
            ImportHelper.FindColumn(headerMap, "Funding offer"),
            ImportHelper.FindColumn(headerMap, "In Scope"),
            ImportHelper.FindColumn(headerMap, "Comments")
        );
    }

    private sealed record ColumnNames(
        string? Qan,
        string? Title,
        string? AwardingOrganisation,
        string? GLH,
        string? SSA,
        string? Route,
        string? FundingOffer,
        string? InScope,
        string? Comments
    );

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

    private static string GetCellTextByColumn(WorksheetPart worksheetPart, string rowIndex, string? column, SharedStringTable? sharedStrings)
    {
        if (string.IsNullOrWhiteSpace(column) || string.IsNullOrWhiteSpace(rowIndex)) return string.Empty;
        var address = $"{column}{rowIndex}";
        var cell = worksheetPart.Worksheet.Descendants<Cell>().FirstOrDefault(c => string.Equals((c.CellReference ?? "").Value, address, StringComparison.OrdinalIgnoreCase));
        if (cell == null) return string.Empty;
        return ImportHelper.GetCellText(cell, sharedStrings)?.Trim() ?? string.Empty;
    }
}
