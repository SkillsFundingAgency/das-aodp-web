using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Moq;
using SFA.DAS.AODP.Application.Helpers;

namespace SFA.DAS.AODP.Application.UnitTests.Helpers;

public class ImportHelperTests
{
    private static Cell CreateCell(string reference, string value, CellValues? dataType = null)
    {
        var cell = new Cell() { CellReference = reference };
        if (dataType.HasValue)
            cell.DataType = new EnumValue<CellValues>(dataType.Value);

        if (dataType == CellValues.InlineString)
        {
            // Inline string should use an InlineString element with a Text child.
            // Setting OpenXmlCompositeElement.InnerText is not allowed because the setter is inaccessible.
            cell.AppendChild(new InlineString(new Text(value)));
        }
        else
        {
            cell.CellValue = new CellValue(value);
        }

        return cell;
    }

    [Fact]
    public void GetCellText_NullCell_ReturnsEmpty()
    {
        var result = ImportHelper.GetCellText(null!, null);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetCellText_InlineString_ReturnsInnerText()
    {
        var cell = CreateCell("A1", "inline text", CellValues.InlineString);
        var text = ImportHelper.GetCellText(cell, null);
        Assert.Equal("inline text", text);
    }

    [Fact]
    public void GetCellText_NoDataType_ReturnsCellValue()
    {
        var cell = CreateCell("A1", "42", null);
        var text = ImportHelper.GetCellText(cell, null);
        Assert.Equal("42", text);
    }

    [Fact]
    public void GetCellText_SharedString_ReturnsSharedStringValue()
    {
        // Build shared string table with two items
        var sst = new SharedStringTable();
        sst.AppendChild(new SharedStringItem(new Text("First")));
        sst.AppendChild(new SharedStringItem(new Text("Second")));

        // Cell references index 1 -> "Second"
        var cell = CreateCell("A1", "1", CellValues.SharedString);

        var text = ImportHelper.GetCellText(cell, sst);
        Assert.Equal("Second", text);
    }

    [Fact]
    public void GetCellText_SharedString_IndexOutOfRange_ReturnsEmpty()
    {
        var sst = new SharedStringTable();
        sst.AppendChild(new SharedStringItem(new Text("OnlyOne")));

        // Index 5 does not exist -> method will return empty string
        var cell = CreateCell("A1", "5", CellValues.SharedString);

        var text = ImportHelper.GetCellText(cell, sst);
        Assert.Equal(string.Empty, text);
    }

    [Fact]
    public void GetCellText_SharedString_NonParsableIndex_ReturnsOriginalValue()
    {
        var sst = new SharedStringTable();
        sst.AppendChild(new SharedStringItem(new Text("OnlyOne")));

        // Non-integer index returns original value per implementation
        var cell = CreateCell("A1", "not-an-int", CellValues.SharedString);

        var text = ImportHelper.GetCellText(cell, sst);
        Assert.Equal("not-an-int", text);
    }

    [Fact]
    public void GetCellText_BooleanMapsValues()
    {
        var trueCell = CreateCell("A1", "1", CellValues.Boolean);
        var falseCell = CreateCell("A2", "0", CellValues.Boolean);
        var otherCell = CreateCell("A3", "X", CellValues.Boolean);

        Assert.Equal("TRUE", ImportHelper.GetCellText(trueCell, null));
        Assert.Equal("FALSE", ImportHelper.GetCellText(falseCell, null));
        Assert.Equal("X", ImportHelper.GetCellText(otherCell, null));
    }

    [Fact]
    public void MapFindColumn_ExactAndContains_MatchesExpected()
    {
        var headerMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["A"] = "Full Name",
            ["B"] = "Some other header",
            ["C"] = "Date of birth"
        };

        // exact match (case-insensitive)
        var exact = ImportHelper.FindColumn(headerMap, "Full Name");
        Assert.Equal("A", exact);

        // contains match (partial)
        var contains = ImportHelper.FindColumn(headerMap, "date");
        Assert.Equal("C", contains);

        // variant array with multiple possibilities
        var multi = ImportHelper.FindColumn(headerMap, "missing", "some other");
        Assert.Equal("B", multi);
    }

    [Fact]
    public void FindColumn_NullOrEmptyInputs_ReturnsNull()
    {
        Assert.Null(ImportHelper.FindColumn(null!, "x"));
        Assert.Null(ImportHelper.FindColumn(new Dictionary<string, string>(), Array.Empty<string>()));
        Assert.Null(ImportHelper.FindColumn(new Dictionary<string, string> { ["A"] = "" }, "x"));
    }

    [Fact]
    public void ValidateRequest_FileNullOrZeroLength_ReturnsInvalid()
    {
        var (isValid, message) = ImportHelper.ValidateRequest(null, "something.xlsx");
        Assert.False(isValid);
        Assert.Null(message);

        var mockFile = new Mock<IFormFile>();
        mockFile.SetupGet(f => f.Length).Returns(0L);
        var (isValid2, message2) = ImportHelper.ValidateRequest(mockFile.Object, "something.xlsx");
        Assert.False(isValid2);
        Assert.Null(message2);
    }

    [Fact]
    public void ValidateRequest_WrongFileExtension_ReturnsUnsupportedMessage()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.SetupGet(f => f.Length).Returns(100L);

        var (isValid, message) = ImportHelper.ValidateRequest(mockFile.Object, "file.xls");
        Assert.False(isValid);
        Assert.Equal("Unsupported file type. Only .xlsx files are accepted.", message);

        var (isValid2, message2) = ImportHelper.ValidateRequest(mockFile.Object, null);
        Assert.False(isValid2);
        Assert.Equal("Unsupported file type. Only .xlsx files are accepted.", message2);
    }

    [Fact]
    public void ValidateRequest_ValidFile_ReturnsValid()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.SetupGet(f => f.Length).Returns(1L);

        var (isValid, message) = ImportHelper.ValidateRequest(mockFile.Object, "file.xlsx");
        Assert.True(isValid);
        Assert.Null(message);

        // Case-insensitive extension
        var (isValid2, message2) = ImportHelper.ValidateRequest(mockFile.Object, "file.XLSX");
        Assert.True(isValid2);
        Assert.Null(message2);
    }

    [Fact]
    public void BuildHeaderMap_ParsesColumnsAndTexts()
    {
        var shared = new SharedStringTable();
        // Not using shared strings for this test - values are in CellValue
        var headerRow = new Row();
        headerRow.Append(CreateCell("A1", "Name"));
        headerRow.Append(CreateCell("B1", "Age"));

        var map = ImportHelper.BuildHeaderMap(headerRow, null);
        Assert.Equal(2, map.Count);
        Assert.Equal("Name", map["A"]);
        Assert.Equal("Age", map["B"]);
    }

    [Fact]
    public void DetectHeaderRow_FindsHeader_WhenKeywordMatches()
    {
        var rows = new List<Row>();

        // Row 0 - empty
        rows.Add(new Row());

        // Row 1 - not header
        var r1 = new Row();
        r1.Append(CreateCell("A2", "foo"));
        rows.Add(r1);

        // Row 2 - header contains keyword
        var r2 = new Row();
        r2.Append(CreateCell("A3", "Candidate Name"));
        r2.Append(CreateCell("B3", "Other"));
        rows.Add(r2);

        var (headerRow, headerIndex) = ImportHelper.DetectHeaderRow(rows, null, new[] { "candidate" }, defaultRowIndex: 0, minMatches: 1);
        Assert.Same(r2, headerRow);
        Assert.Equal(2, headerIndex);
    }

    [Fact]
    public void GetColumnName_ParsesLettersOnly()
    {
        Assert.Equal("A", ImportHelper.GetColumnName("A1"));
        Assert.Equal("AB", ImportHelper.GetColumnName("AB12"));
        Assert.Null(ImportHelper.GetColumnName(""));
        Assert.Null(ImportHelper.GetColumnName(null));
        Assert.Equal("XYZ", ImportHelper.GetColumnName("XYZ"));
        Assert.Equal("C", ImportHelper.GetColumnName("C"));
    }
}
