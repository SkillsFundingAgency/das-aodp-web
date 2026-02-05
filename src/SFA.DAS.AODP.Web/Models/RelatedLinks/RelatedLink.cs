namespace SFA.DAS.AODP.Web.Models.RelatedLinks
{
    public class RelatedLink
    {
        public string Text { get; }
        public string Url { get; }
        public bool OpenInNewTab { get; }

        public RelatedLink(string text, string url, bool openInNewTab = true)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            OpenInNewTab = openInNewTab;
        }
    }
}
