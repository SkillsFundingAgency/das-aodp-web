using SFA.DAS.AODP.Application.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    // to do refactoring later
    public sealed record BespokeQualificationCandidateColumnNames(
        string? QualificationNumber,
        string? Title,
        string? AwardingOrganisation,
        string? FundingOffer,
        string? FundingApprovalEndDate
    );

    public class ImportFileValidationOptions
    {
        public int DefaultRowIndex { get; set; } = 0;
        public int MinMatches { get; set; } = 1;
        public Func<IDictionary<string, string>, object> MapColumns { get; set; } = _ => new object();
    }

    [ExcludeFromCodeCoverage] // to do refactoring later
    public class UploadBespokeQualificationCandidatesViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "You must select a CSV file")]
        public IFormFile File { get; set; }

        private const string GenericUploadErrorMessage =
            "The file you provided does not match the required format.";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return ValidateInternal(validationContext).GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<ValidationResult>> ValidateInternal(ValidationContext validationContext)
        {
            if (File == null)
            {
                return new[]
                {
                    new ValidationResult("You must select a CSV file.", new[] { nameof(File) })
                };
            }

            var ext = Path.GetExtension(File.FileName)?.ToLowerInvariant();
            if (ext != ".csv")
            {
                return new[]
                {
                    new ValidationResult("This must be a CSV file.", new[] { nameof(File) })
                };
            }

            if (File.Length == 0)
            {
                return new[]
                {
                    new ValidationResult("You must select a CSV file.", new[] { nameof(File) })
                };
            }

            var headerKeywords = new[]
            {
                "qualification number",
                "title",
                "awarding organisation",
                "funding offer",
                "funding approval end date"
            };

            bool isValid;

            try
            {
                isValid = await ValidateCsvImportFile(
                    file: File,
                    headerKeywords: headerKeywords,
                    options: new ImportFileValidationOptions
                    {
                        DefaultRowIndex = 1,
                        MinMatches = 3,
                        MapColumns = MapBespokeQualificationCandidateColumns
                    },
                    cancellationToken: CancellationToken.None
                );
            }
            catch
            {
                return new[]
                {
                    new ValidationResult(GenericUploadErrorMessage, new[] { nameof(File) })
                };
            }

            if (!isValid)
            {
                return new[]
                {
                    new ValidationResult(GenericUploadErrorMessage, new[] { nameof(File) })
                };
            }

            return Enumerable.Empty<ValidationResult>();
        }

        private static BespokeQualificationCandidateColumnNames MapBespokeQualificationCandidateColumns(IDictionary<string, string> headerMap)
        {
            return new BespokeQualificationCandidateColumnNames(
                FindColumn(headerMap, "Qualification number"),
                FindColumn(headerMap, "Title"),
                FindColumn(headerMap, "Awarding organisation"),
                FindColumn(headerMap, "Funding offer"),
                FindColumn(headerMap, "Funding approval end date")
            );
        }

        public async Task<bool> ValidateCsvImportFile(IFormFile? file, string[] headerKeywords, ImportFileValidationOptions options, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return false;

            var rows = new List<string[]>();

            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();

                    if (!string.IsNullOrWhiteSpace(line))
                        rows.Add(ParseCsvLine(line));
                }
            }

            //if (rows.Count <= 1)
            //    return false;

            var (headerRow, headerIndex) = DetectHeaderRowCsv(
                rows,
                headerKeywords,
                options.DefaultRowIndex,
                options.MinMatches);

            if (headerIndex < 0)
                return false;

            // Step 4 — build header map
            var headerMapCsv = BuildHeaderMapCsv(headerRow);

            // Step 5 — convert int → string for your mapper
            var headerMap = headerMapCsv.ToDictionary(
                x => x.Key,
                x => x.Value.ToString(),
                StringComparer.OrdinalIgnoreCase
            );

            // Step 6 — map columns using your supplied function
            var columns = options.MapColumns(headerMap);

            // Step 7 — detect missing columns using your record
            var missingColumns = columns.GetType()
                .GetProperties()
                .Where(p => p.PropertyType == typeof(string))
                .Select(p => new { p.Name, Value = (string?)p.GetValue(columns) })
                .Where(x => string.IsNullOrWhiteSpace(x.Value))
                .Select(x => x.Name)
                .ToList();

            return missingColumns.Count == 0;
        }

        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var field = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (c == ',' && !inQuotes)
                {
                    result.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }

            result.Add(field.ToString());
            return result.ToArray();
        }

        public static (string[] headerRow, int headerIndex) DetectHeaderRowCsv(List<string[]> rows, string[] headerKeywords, int defaultRowIndex = 0, int minMatches = 1)
        {
            if (rows.Count == 0)
                return (Array.Empty<string>(), -1);

            string[] headerRow =
                defaultRowIndex >= 0 && defaultRowIndex < rows.Count
                    ? rows[defaultRowIndex]
                    : rows[0];

            int headerIndex = rows.IndexOf(headerRow);

            for (int r = 0; r < rows.Count; r++)
            {
                var values = rows[r]
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim().ToLowerInvariant())
                    .ToList();

                if (values.Count == 0)
                    continue;

                int matches = values.Count(v =>
                    headerKeywords.Any(k => v.Contains(k.ToLowerInvariant()))
                );

                if (matches >= minMatches)
                    return (rows[r], r);
            }

            return (headerRow, headerIndex);
        }

        public static Dictionary<string, int> BuildHeaderMapCsv(string[] headerRow)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < headerRow.Length; i++)
            {
                var key = headerRow[i]?.Trim();
                if (!string.IsNullOrWhiteSpace(key) && !map.ContainsKey(key))
                    map[key] = i;
            }

            return map;
        }

        public static string? FindColumn(IDictionary<string, string> headerMap, string headerName)
        {
            // Try direct lookup first — dictionary is case-insensitive
            if (headerMap.TryGetValue(headerName, out var value))
                return value;

            // Normalize both sides in a consistent way
            string norm = NormalizeHeader(headerName);

            // Try normalized lookup
            if (headerMap.TryGetValue(norm, out value))
                return value;

            // Final fallback — scan keys in case dictionary keys are not normalized
            foreach (var kv in headerMap)
            {
                if (NormalizeHeader(kv.Key) == norm)
                    return kv.Value;
            }

            return null;
        }

        private static string NormalizeHeader(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;

            var t = s.Trim();

            // Remove BOM
            t = t.TrimStart('\uFEFF');

            // Replace non-breaking spaces
            t = t.Replace('\u00A0', ' ');

            // Collapse internal whitespace
            t = System.Text.RegularExpressions.Regex.Replace(t, @"\s+", " ", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromMilliseconds(100));

            // Lowercase
            t = t.ToLowerInvariant();

            return t;
        }
    }
}