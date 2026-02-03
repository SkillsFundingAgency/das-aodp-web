namespace SFA.DAS.AODP.Web.Models.Import;

public class ImportFileValidationOptions
{
    public string[] HeaderKeywords { get; set; } = Array.Empty<string>();
    public string TargetSheetName { get; set; } = string.Empty;
    public int DefaultRowIndex { get; set; }
    public int MinMatches { get; set; }
    public Func<IDictionary<string, string>, object> MapColumns { get; set; } = _ => new object();
}
