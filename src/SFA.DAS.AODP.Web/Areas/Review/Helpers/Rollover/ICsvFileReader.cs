namespace SFA.DAS.AODP.Web.Areas.Review.Helpers.Rollover
{
    public interface ICsvFileReader
    {
        Task<CsvFileReaderResult<T>> FileReadAsync<T>(
            IFormFile? file,
            IEnumerable<string> requiredHeaders,
            Func<IReadOnlyDictionary<string, string>, T> mapRow);

        // For reading a file back from blob storage (e.g. after waiting for a malware scan)
        // rather than directly off the original upload request. Takes ownership of the stream
        // and disposes it once read.
        Task<CsvFileReaderResult<T>> FileReadAsync<T>(
            Stream stream,
            string fileName,
            long length,
            IEnumerable<string> requiredHeaders,
            Func<IReadOnlyDictionary<string, string>, T> mapRow);
    }
}
