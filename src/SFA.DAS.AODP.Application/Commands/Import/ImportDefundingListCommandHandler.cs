using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MediatR;
using SFA.DAS.AODP.Application.Helpers;

namespace SFA.DAS.AODP.Application.Commands.Import;

public class ImportDefundingListCommandHandler : IRequestHandler<ImportDefundingListCommand, BaseMediatrResponse<ImportDefundingListCommandResponse>>
{
    private const string GenericErrorMessage = "The file you provided does not match the required format for a defunding list.";
    public async Task<BaseMediatrResponse<ImportDefundingListCommandResponse>> Handle(ImportDefundingListCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<ImportDefundingListCommandResponse>();

        try
        {
            // Validate request and file type
            var (IsValid, ErrorMessage) = ImportHelper.ValidateRequest(request.File, request.FileName);
            if (!IsValid)
            {
                response.Success = false;
                response.ErrorMessage = ErrorMessage;
                response.Value = new ImportDefundingListCommandResponse { ImportedCount = 0 };
                return response;
            }

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
            var rows = ImportHelper.GetRowsFromWorksheet(worksheetPart).ToList();
            if (rows.Count <= 1)
            {
                response.Success = false;
                response.ErrorMessage = GenericErrorMessage;
                response.Value = new ImportDefundingListCommandResponse { ImportedCount = 0 };
                return response;
            }

            // Detect header row
            var headerKeywords = new[] { "qualification", "qan", "title", "award", "guided", "sector", "route", "funding", "in scope", "comments" };
            var (headerRow, headerIndex) = ImportHelper.DetectHeaderRow(rows, sharedStrings, headerKeywords, defaultRowIndex: 6, minMatches: 2);

            if (headerIndex < 0)
            {
                response.Success = false;
                response.ErrorMessage = GenericErrorMessage;
                response.Value = new ImportDefundingListCommandResponse { ImportedCount = 0 };
                return response;
            }

            // Build header map
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
}
