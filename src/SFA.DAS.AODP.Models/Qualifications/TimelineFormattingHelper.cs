using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SFA.DAS.AODP.Models.Qualifications;

[ExcludeFromCodeCoverage]
public static class TimelineFormattingHelper
{
    public static string ToDisplayString(List<string> items, string singleItemPrefix, string multipleItemsTitle)
    {
        var filteredItems = items
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (filteredItems.Count == 0)
        {
            return string.Empty;
        }

        if (filteredItems.Count == 1)
        {
            return $"{singleItemPrefix}{filteredItems[0]}";
        }

        var sb = new StringBuilder();
        sb.Append(multipleItemsTitle);
        sb.Append("<ul>");

        foreach (var item in filteredItems)
        {
            sb.Append("<li>");
            sb.Append(item);
            sb.Append("</li>");
        }

        sb.Append("</ul>");

        return sb.ToString();
    }
}