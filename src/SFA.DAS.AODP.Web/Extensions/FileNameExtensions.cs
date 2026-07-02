namespace SFA.DAS.AODP.Web.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class FileNameExtensions
    {
        public static string SanitiseFileName(this string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            return name.Trim();
        }
    }

}
