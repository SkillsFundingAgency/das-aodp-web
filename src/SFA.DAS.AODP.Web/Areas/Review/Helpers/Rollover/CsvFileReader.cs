using System.Text;
using System.Text.RegularExpressions;

namespace SFA.DAS.AODP.Web.Areas.Review.Helpers.Rollover
{
    public class CsvFileReader : ICsvFileReader
    {
        private const string EmptyFileErrorMessage =
            "The selected file is empty. Upload a CSV file that contains data.";

        private const string MissingFileErrorMessage =
            "You must select a CSV file.";

        private const string WrongExtensionMessage =
            "This must be a CSV file.";

        private const string FileFormatErrorMessage =
            "The file you provided does not match the required format.";

        public async Task<CsvFileReaderResult<T>> FileReadAsync<T>(
            IFormFile? file,
            IEnumerable<string> requiredHeaders,
            Func<IReadOnlyDictionary<string, string>, T> mapRow)
        {
            var result = new CsvFileReaderResult<T>();

            if (file == null)
            {
                result.Errors.Add(MissingFileErrorMessage);
                return result;
            }

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (ext != ".csv")
            {
                result.Errors.Add(WrongExtensionMessage);
                return result;
            }

            if (file.Length == 0)
            {
                result.Errors.Add(EmptyFileErrorMessage);
                return result;
            }

            var rows = await ReadRowsAsync(file);

            var headerRow = rows[0];
            var headers = headerRow.Select((h, i) => new
            {
                Normalized = Normalize(h),
                Index = i
            }).ToList();

            foreach (var required in requiredHeaders)
            {
                var normalizedRequired = Normalize(required);

                if (!headers.Any(h =>
                        h.Normalized.Equals(normalizedRequired, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Errors.Add(FileFormatErrorMessage);
                    return result;
                }
            }

            var headerMap = headers.ToDictionary(h => h.Normalized, h => h.Index);

            foreach (var row in rows.Skip(1))
            {
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var h in headerMap)
                {
                    var value = h.Value < row.Length ? row[h.Value] : "";
                    dict[h.Key] = value?.Trim() ?? "";
                }

                result.Items.Add(mapRow(dict));
            }

            if (result.Items.Count == 0)
            {
                result.Errors.Add(EmptyFileErrorMessage);
                return result;
            }

            return result;
        }

        private async Task<List<string[]>> ReadRowsAsync(IFormFile file, CancellationToken ct = default)
        {
            var rows = new List<string[]>();

            if (file == null || file.Length == 0)
                return rows;

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(line))
                    rows.Add(ParseCsv(line));
            }

            return rows;
        }

        private static string Normalize(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;

            var t = s.Trim();
            t = t.TrimStart('\uFEFF');
            t = t.Replace('\u00A0', ' ');
            t = Regex.Replace(t, @"\s+", " ");
            return t.ToLowerInvariant();
        }

        private static string[] ParseCsv(string line)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            foreach (char c in line)
            {
                if (c == '"') { inQuotes = !inQuotes; continue; }

                if (c == ',' && !inQuotes)
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                }
                else sb.Append(c);
            }

            result.Add(sb.ToString());
            return result.ToArray();
        }
    }
}