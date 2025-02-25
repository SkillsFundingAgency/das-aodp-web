using System.Web;

namespace SFA.DAS.AODP.Web.Helpers.Markdown
{
    public static class MarkdownHelper
    {
        public static string ToGovUkHtml(string? markdown)
        {
            if (string.IsNullOrEmpty(markdown)) return string.Empty;
            return Markdig.Markdown
                .ToHtml(HttpUtility.HtmlEncode(markdown))
                .Replace("<a", "<a class=\"govuk-link\" target=\"_blank\"");
        }

    }
}
