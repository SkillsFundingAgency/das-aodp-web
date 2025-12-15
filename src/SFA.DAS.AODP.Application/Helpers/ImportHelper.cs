using System.Text;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using DocumentFormat.OpenXml.Packaging;

namespace SFA.DAS.AODP.Application.Helpers;

public static class ImportHelper
{
    public static string GetCellText(Cell cell, SharedStringTable? sharedStrings)
    {
        if (cell == null)
        {
            return string.Empty;
        }

        if (cell.DataType != null && cell.DataType.Value == CellValues.InlineString)
        {
            return cell.InnerText ?? string.Empty;
        }

        var value = cell.CellValue?.InnerText ?? string.Empty;

        if (cell.DataType == null)
        {
            return value;
        }

        return GetTypedCellValue(cell, value, sharedStrings);

    }

    public static string? FindColumn(IDictionary<string, string> headerMap, params string[] variants)
    {
        if (headerMap == null || variants == null || variants.Length == 0) return null;

        // exact match first
        foreach (var kv in headerMap)
        {
            var header = kv.Value?.Trim();
            if (string.IsNullOrEmpty(header)) continue;

            if (variants.Any(v => string.Equals(v.Trim(), header, StringComparison.OrdinalIgnoreCase)))
                return kv.Key;
        }

        // contains match
        foreach (var kv in headerMap)
        {
            var header = kv.Value?.Trim().ToLowerInvariant() ?? string.Empty;
            foreach (var v in variants)
            {
                var variant = v?.Trim().ToLowerInvariant() ?? string.Empty;
                if (!string.IsNullOrEmpty(variant) && header.Contains(variant))
                    return kv.Key;
            }
        }

        return null;
    }

    private static string GetTypedCellValue(Cell cell, string value, SharedStringTable? sharedStrings)
    {
        var dataType = cell.DataType?.Value;

        if (dataType != null && dataType.Equals(CellValues.SharedString))
            return GetSharedStringValue(value, sharedStrings);
        if (dataType != null && dataType.Equals(CellValues.Boolean))
            return MapBooleanValue(value);

        return value;
    }

    private static string GetSharedStringValue(string value, SharedStringTable? sharedStrings)
    {
        if (!int.TryParse(value, out var sstIndex) || sharedStrings == null)
            return value;

        var ssi = sharedStrings.Elements<SharedStringItem>().ElementAtOrDefault(sstIndex);
        if (ssi == null)
            return string.Empty;

        // Prefer straightforward inner text if available
        var directText = ssi.InnerText;
        if (!string.IsNullOrEmpty(directText))
            return directText;

        // Otherwise try to compose text from runs
        var runText = ssi
            .Elements<Run>()
            .SelectMany(r => r.Elements<Text>())
            .Select(t => t.Text)
            .FirstOrDefault();

        return runText ?? string.Empty;
    }

    private static string MapBooleanValue(string value) =>
        value switch
        {
            "1" => "TRUE",
            "0" => "FALSE",
            _ => value
        };


    public static (bool IsValid, string? ErrorMessage) ValidateRequest(IFormFile? file, string? fileName)
    {
        if (file == null || file.Length == 0)
            return (false, null);

        if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return (false, "Unsupported file type. Only .xlsx files are accepted.");

        return (true, null);
    }

    public static IEnumerable<Row> GetRowsFromWorksheet(WorksheetPart worksheetPart)
    {
        var sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
        if (sheetData == null) yield break;
        foreach (var row in sheetData.Elements<Row>()) yield return row;
    }

    public static Dictionary<string, string> BuildHeaderMap(Row headerRow, SharedStringTable? sharedStrings)
    {
        var headerMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in headerRow.Elements<Cell>())
        {
            var col = GetColumnName(cell.CellReference?.Value);
            var txt = GetCellText(cell, sharedStrings);
            if (!string.IsNullOrWhiteSpace(col) && !string.IsNullOrWhiteSpace(txt))
                headerMap[col!] = txt.Trim();
        }
        return headerMap;
    }

    public static (Row headerRow, int headerIndex) DetectHeaderRow(List<Row> rows, SharedStringTable? sharedStrings, string[] headerKeywords, int defaultRowIndex = 0, int minMatches = 1)
    {
        Row headerRow;
        if (defaultRowIndex >= 0 && defaultRowIndex < rows.Count)
            headerRow = rows[defaultRowIndex];
        else
            headerRow = rows.Count > 0 ? rows[0] : null!;

        int headerListIndex = rows.IndexOf(headerRow);

        for (int r = 0; r < Math.Min(rows.Count, 12); r++)
        {
            var cellTexts = rows[r].Elements<Cell>()
                .Select(c => GetCellText(c, sharedStrings).Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.ToLowerInvariant())
                .ToList();

            if (cellTexts.Count == 0) continue;

            var matches = cellTexts.Count(ct => headerKeywords.Any(k => ct.Contains(k)));
            if (matches >= minMatches)
            {
                headerRow = rows[r];
                headerListIndex = r;
                break;
            }
        }

        return (headerRow, headerListIndex);
    }

    public static string? GetColumnName(string? cellReference)
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
}
