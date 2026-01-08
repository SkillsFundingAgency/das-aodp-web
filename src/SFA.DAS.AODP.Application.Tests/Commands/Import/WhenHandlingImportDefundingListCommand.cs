using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using SFA.DAS.AODP.Application.Commands.Import;
using System.Text;

namespace SFA.DAS.AODP.Application.UnitTests.Commands.Import;

public class WhenHandlingImportDefundingListCommand
{
    private const string TargetSheetName = "Approval not extended";
    private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string GenericErrorMessage = "The file you provided does not match the required format for a defunding list.";

    [Fact]
    public async Task FileNull_ReturnsFailure_WithNullErrorMessage()
    {
        // Arrange
        var handler = new ImportDefundingListCommandHandler();
        var request = new ImportDefundingListCommand
        {
            File = null,
            FileName = null
        };

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Value);
        Assert.Equal(0, response.Value.ImportedCount);
    }

    [Fact]
    public async Task UnsupportedFileType_ReturnsUnsupportedFileTypeMessage()
    {
        // Arrange
        var handler = new ImportDefundingListCommandHandler();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("dummy"));
        var formFile = new FormFile(stream, 0, stream.Length, "file", "file.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };
        var request = new ImportDefundingListCommand
        {
            File = formFile,
            FileName = formFile.FileName
        };

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Unsupported file type. Only .xlsx files are accepted.", response.ErrorMessage);
        Assert.NotNull(response.Value);
        Assert.Equal(0, response.Value.ImportedCount);
    }

    [Fact]
    public async Task SheetNotFound_ReturnsFormatError()
    {
        // Arrange - create a valid xlsx with a DIFFERENT sheet name
        using var ms = CreateSpreadsheetStream("NotTheRightSheet", new List<List<string>> {
                new List<string> { "Some", "Data" },
                new List<string> { "More", "Data" }
            });

        var formFile = new FormFile(ms, 0, ms.Length, "file", "file.xlsx")
        {
            Headers = new HeaderDictionary(),
            ContentType = XlsxContentType
        };

        var handler = new ImportDefundingListCommandHandler();
        var request = new ImportDefundingListCommand
        {
            File = formFile,
            FileName = formFile.FileName
        };

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal(GenericErrorMessage, response.ErrorMessage);
        Assert.NotNull(response.Value);
        Assert.Equal(0, response.Value.ImportedCount);
    }

    [Fact]
    public async Task RowsInsufficient_ReturnsFormatError()
    {
        // Arrange - create target sheet with only one row -> rows.Count <= 1
        using var ms = CreateSpreadsheetStream(TargetSheetName, new List<List<string>> {
                new List<string> { "OnlyOneRowHeader" }
            });

        var formFile = new FormFile(ms, 0, ms.Length, "file", "file.xlsx")
        {
            Headers = new HeaderDictionary(),
            ContentType = XlsxContentType
        };

        var handler = new ImportDefundingListCommandHandler();
        var request = new ImportDefundingListCommand
        {
            File = formFile,
            FileName = formFile.FileName
        };

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal(GenericErrorMessage, response.ErrorMessage);
        Assert.NotNull(response.Value);
        Assert.Equal(0, response.Value.ImportedCount);
    }

    [Fact]
    public async Task HeaderNotDetected_ReturnsFormatError()
    {
        // Arrange - create multiple rows but none matching header keywords (so headerIndex < 0 behaviour not triggered directly,
        var rows = new List<List<string>>
            {
                new List<string> { "random", "values" },
                new List<string> { "no", "match" },
                new List<string> { "still", "nope" },
                new List<string> { "foo", "bar" },
                new List<string> { "baz", "qux" },
                new List<string> { "one", "two" },
                new List<string> { "three", "four" }
            };

        using var ms = CreateSpreadsheetStream(TargetSheetName, rows);

        var formFile = new FormFile(ms, 0, ms.Length, "file", "file.xlsx")
        {
            Headers = new HeaderDictionary(),
            ContentType = XlsxContentType
        };

        var handler = new ImportDefundingListCommandHandler();
        var request = new ImportDefundingListCommand
        {
            File = formFile,
            FileName = formFile.FileName
        };

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert -> missing columns expected
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal(GenericErrorMessage, response.ErrorMessage);
        Assert.NotNull(response.Value);
        Assert.Equal(0, response.Value.ImportedCount);
    }

    [Fact]
    public async Task MissingColumns_ReturnsFormatError()
    {
        // Arrange
        var headerRow = new List<string>
            {
                "Qualification number",
                "Title",
            };

        var rows = new List<List<string>>
            {
                new List<string> { "" },
                headerRow
            };

        using var ms = CreateSpreadsheetStream(TargetSheetName, rows);

        var formFile = new FormFile(ms, 0, ms.Length, "file", "file.xlsx")
        {
            Headers = new HeaderDictionary(),
            ContentType = XlsxContentType
        };

        var handler = new ImportDefundingListCommandHandler();
        var request = new ImportDefundingListCommand
        {
            File = formFile,
            FileName = formFile.FileName
        };

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal(GenericErrorMessage, response.ErrorMessage);
        Assert.NotNull(response.Value);
        Assert.Equal(0, response.Value.ImportedCount);
    }

    [Fact]
    public async Task ValidHeaderRow_ReturnsSuccess()
    {
        // Arrange
        var headers = new List<string>
            {
                "Qualification number",
                "Title",
                "Awarding organisation",
                "Guided Learning Hours",
                "Sector Subject Area Tier 2",
                "Relevant route",
                "Funding offer",
                "In Scope",
                "Comments"
            };

        var rows = new List<List<string>>
            {
                new List<string> { "" },
                headers
            };

        using var ms = CreateSpreadsheetStream(TargetSheetName, rows);

        var formFile = new FormFile(ms, 0, ms.Length, "file", "Defunding.xlsx")
        {
            Headers = new HeaderDictionary(),
            ContentType = XlsxContentType
        };

        var handler = new ImportDefundingListCommandHandler();
        var request = new ImportDefundingListCommand
        {
            File = formFile,
            FileName = formFile.FileName
        };

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.NotNull(response.Value);
        Assert.Equal(0, response.Value.ImportedCount);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task InvalidExcelThrows_IsCaughtAndReturnedAsErrorMessage()
    {
        // Arrange - bytes that are not a valid xlsx but extension is .xlsx
        var bytes = Encoding.UTF8.GetBytes("this is not a zip/xlsx");
        using var ms = new MemoryStream(bytes);
        var formFile = new FormFile(ms, 0, ms.Length, "file", "file.xlsx")
        {
            Headers = new HeaderDictionary(),
            ContentType = XlsxContentType
        };

        var handler = new ImportDefundingListCommandHandler();
        var request = new ImportDefundingListCommand
        {
            File = formFile,
            FileName = formFile.FileName
        };

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorMessage);
        Assert.NotEmpty(response.ErrorMessage);
        Assert.NotNull(response.Value);
        Assert.Equal(0, response.Value.ImportedCount);
    }

    #region Helpers

    private static MemoryStream CreateSpreadsheetStream(string sheetName, List<List<string>> rows)
    {
        var ms = new MemoryStream();
        using (var document = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var sstPart = workbookPart.AddNewPart<SharedStringTablePart>();
            sstPart.SharedStringTable = new SharedStringTable();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            worksheetPart.Worksheet = new Worksheet(sheetData);

            var sst = sstPart.SharedStringTable!;
            var sharedStringIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int nextIndex = 0;

            for (int r = 0; r < rows.Count; r++)
            {
                var row = new Row() { RowIndex = (UInt32)(r + 1) };
                var cols = rows[r];
                for (int c = 0; c < cols.Count; c++)
                {
                    var text = cols[c] ?? string.Empty;
                    if (!sharedStringIndex.TryGetValue(text, out var idx))
                    {
                        sst.AppendChild(new SharedStringItem(new Text(text)));
                        sharedStringIndex[text] = nextIndex;
                        idx = nextIndex;
                        nextIndex++;
                    }

                    var cell = new Cell
                    {
                        CellReference = GetCellReference(c + 1, r + 1),
                        DataType = CellValues.SharedString,
                        CellValue = new CellValue(idx.ToString())
                    };
                    row.AppendChild(cell);
                }
                sheetData.AppendChild(row);
            }

            sst.Save();
            worksheetPart.Worksheet.Save();

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            var sheet = new Sheet()
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = sheetName
            };
            sheets.Append(sheet);

            workbookPart.Workbook.Save();
        }

        ms.Position = 0;
        return ms;
    }

    private static string GetCellReference(int colIndex, int rowIndex)
    {
        return $"{GetColumnLetter(colIndex)}{rowIndex}";
    }

    private static string GetColumnLetter(int colIndex)
    {
        var dividend = colIndex;
        var columnName = string.Empty;
        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar('A' + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }
        return columnName;
    }

    #endregion
}
