using DocumentFormat.OpenXml.Spreadsheet;

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
}
