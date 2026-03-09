namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public interface ICsvFileReader
    {
        Task<CsvFileReaderResult<T>> FileReadAsync<T>(
            IFormFile file,
            IEnumerable<string> requiredHeaders,
            Func<IReadOnlyDictionary<string, string>, T> mapRow);
    }
}
