using DocumentFormat.OpenXml.Spreadsheet;

namespace SFA.DAS.AODP.Web.Models.Qualifications
{
    public class EligibleForFundingStatus
    {
        public bool IsEligible { get; }
        public List<string> IneligibleReasons { get; }

        private static readonly Dictionary<string, string> FieldNameMap = new()
        {
            { "OfferedInEngland", "offered in England is set to false" },
            { "IntentionToSeekFundingInEngland", "intention to seek funding in England is set to false" },
            { "Type", "qualification type is not eligible" },
            { "Title", "qualification title is excluded by common funding approval principles" },
            { "Glh", "guided learning hours (GLH) is set to zero" },
            { "Tqt", "total qualification time (TQT) is set to zero" },
            { "TqtLessThanGlh", "total qualification time (TQT) is less than guided learning hours (GLH)" },
        };

        public EligibleForFundingStatus(bool? isEligible, string? csvReasons)
        {
            IsEligible = isEligible ?? false;

            IneligibleReasons = string.IsNullOrWhiteSpace(csvReasons)
                ? new List<string>()
                : csvReasons.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Select(field => FieldNameMap.TryGetValue(field, out var label) ? label : field)
                    .ToList();
        }

        public bool HasMultipleReasons => IneligibleReasons.Count > 1;
        public bool HasSingleReason => IneligibleReasons.Count == 1;

        public string SummaryMessage => HasSingleReason
            ? $"Not eligible because {IneligibleReasons[0]}"
            : "Not eligible because:";

        public string ToDisplayString(string separator = "<br/>")
        {
            if (IsEligible || IneligibleReasons.Count == 0)
            {
                return string.Empty;
            }

            if (HasSingleReason)
            {
                return SummaryMessage;
            }

            return $"{SummaryMessage}{separator}{separator}{string.Join(separator, IneligibleReasons)}";
        }
    }
}
