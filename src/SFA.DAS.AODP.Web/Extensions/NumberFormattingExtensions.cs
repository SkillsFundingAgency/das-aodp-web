namespace SFA.DAS.AODP.Web.Extensions
{
    public static class NumberFormattingExtensions
    {
        public static string ToSmartNumber(this decimal? value)
        {
            if (value == null)
                return "Not answered";

            var v = value.Value;

            // Whole number → no decimals
            if (v % 1 == 0)
                return v.ToString("0");

            // Has decimals → show only necessary decimals
            return v.ToString("0.########");
        }
    }

}
