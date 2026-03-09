namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public class CsvFileReaderResult<T>
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; } = new();
        public List<T> Items { get; } = new();
    }
}