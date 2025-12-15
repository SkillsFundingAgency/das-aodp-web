using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using SFA.DAS.AODP.Application.Commands.Import;
using System.Text;

namespace SFA.DAS.AODP.Application.UnitTests.Commands.Import;

public class WhenHandlingImportPldnsCommand
{
    private const string PldnsSheetName = "PLDNS V12F";
    private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [Fact]
    public async Task FileNull_ReturnsFailure_WithNullErrorMessage()
    {
        // Arrange
        var handler = new ImportPldnsCommandHandler();
        var request = new ImportPldnsCommand
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
        var handler = new ImportPldnsCommandHandler();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("dummy"));
        var formFile = new FormFile(stream, 0, stream.Length, "file", "file.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };
        var request = new ImportPldnsCommand
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
                new List<string> { "Some", "Data" }
            });

        var formFile = new FormFile(ms, 0, ms.Length, "file", "file.xlsx")
        {
            Headers = new HeaderDictionary(),
            ContentType = XlsxContentType
        };

        var handler = new ImportPldnsCommandHandler();
        var request = new ImportPldnsCommand
        {
            File = formFile,
            FileName = formFile.FileName
        };

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("The selected file must use the correct format", response.ErrorMessage);
        Assert.NotNull(response.Value);
        Assert.Equal(0, response.Value.ImportedCount);
    }

    [Fact]
    public async Task RowsInsufficient_ReturnsFormatError()
    {
        // Arrange - create sheet with only one row -> rows.Count <= 1
        using var ms = CreateSpreadsheetStream(PldnsSheetName, new List<List<string>> {
                new List<string> { "OnlyOneRowHeader" }
            });

        var formFile = new FormFile(ms, 0, ms.Length, "file", "file.xlsx")
        {
            Headers = new HeaderDictionary(),
            ContentType = XlsxContentType
        };

        var handler = new ImportPldnsCommandHandler();
        var request = new ImportPldnsCommand
        {
            File = formFile,
            FileName = formFile.FileName
        };

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("The selected file must use the correct format", response.ErrorMessage);
        Assert.NotNull(response.Value);
        Assert.Equal(0, response.Value.ImportedCount);
    }

    [Fact]
    public async Task MissingColumns_ReturnsFormatError()
    {
        // Arrange
        // Create two rows: preamble row and header row that only contains a subset of headers
        var rows = new List<List<string>>
            {
                new List<string> { "" },
                new List<string> { "text QAN", "Date PLDNS list updated", "NOTE" } 
            };

        using var ms = CreateSpreadsheetStream(PldnsSheetName, rows);

        var formFile = new FormFile(ms, 0, ms.Length, "file", "file.xlsx")
        {
            Headers = new HeaderDictionary(),
            ContentType = XlsxContentType
        };

        var handler = new ImportPldnsCommandHandler();
        var request = new ImportPldnsCommand
        {
            File = formFile,
            FileName = formFile.FileName
        };

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("The selected file must use the correct format", response.ErrorMessage);
        Assert.NotNull(response.Value);
        Assert.Equal(0, response.Value.ImportedCount);
    }

    [Fact]
    public async Task ValidHeaderRow_ReturnsSuccess()
    {
        // Arrange
        var headers = new List<string>
            {
                "text QAN",
                "Date PLDNS list updated",
                "NOTE",
                "PLDNS 14-16",
                "NOTES PLDNS 14-16",
                "PLDNS 16-19",
                "NOTES PLDNS 16-19",
                "PLDNS Local flex",
                "NOTES PLDNS Local flex",
                "PLDNS Legal entitlement L2/L3",
                "NOTES PLDNS Legal entitlement L2/L3",
                "PLDNS Legal entitlement Eng/Maths",
                "NOTES PLDNS Legal entitlement Eng/Maths",
                "PLDNS Digital entitlement",
                "NOTES PLDNS Digital entitlement",
                "PLDNS ESF L3/L4",
                "NOTES PLDNS ESF L3/L4",
                "PLDNS Loans",
                "NOTES PLDNS Loans",
                "PLDNS Lifelong Learning Entitlement",
                "NOTES PLDNS Lifelong Learning Entitlement",
                "PLDNS Level 3 Free Courses for Jobs",
                "Level 3 Free Courses for Jobs (Previously known as National skills fund L3 extended entitlement)",
                "PLDNS CoF",
                "NOTES  PLDNS CoF",
                "Start date",
                "NOTES Start date"
            };

        var rows = new List<List<string>>
            {
                new List<string> { "" },
                headers
            };

        using var ms = CreateSpreadsheetStream(PldnsSheetName, rows);

        var formFile = new FormFile(ms, 0, ms.Length, "file", "PlDns.xlsx")
        {
            Headers = new HeaderDictionary(),
            ContentType = XlsxContentType
        };

        var handler = new ImportPldnsCommandHandler();
        var request = new ImportPldnsCommand
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
        // Arrange - provide invalid/non-xlsx bytes but with .xlsx extension so ValidateRequest passes
        var bytes = Encoding.UTF8.GetBytes("this is not a zip/xlsx");
        using var ms = new MemoryStream(bytes);
        var formFile = new FormFile(ms, 0, ms.Length, "file", "file.xlsx")
        {
            Headers = new HeaderDictionary(),
            ContentType = XlsxContentType
        };

        var handler = new ImportPldnsCommandHandler();
        var request = new ImportPldnsCommand
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
        // Create the spreadsheet document
        using (var document = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            // shared strings
            var sstPart = workbookPart.AddNewPart<SharedStringTablePart>();
            sstPart.SharedStringTable = new SharedStringTable();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            worksheetPart.Worksheet = new Worksheet(sheetData);

            // add rows and populate shared strings
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
                    // add shared string entry (even for empty strings to keep indexing)
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
