using System.Globalization;

namespace SFA.DAS.AODP.Web.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly CultureInfo LowercaseAmPmCulture = CreateCulture();

        public static string ToDateTimeDisplayFormat(this DateTime dateTime)
        {
            return $"{dateTime.ToString("dd MMMM yyyy")} at {dateTime.ToString("hh:mmtt").ToLower()}";
        }

        public static string ToDateDisplayFormat(this DateTime dateTime)
        {
            return dateTime.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats as a 12 hour date time format with lowercase am/pm designators, e.g. "31 December 2024, 11:59:59 pm"
        /// </summary>
        /// <param name="dateTime">The datetime to format.</param>
        /// <returns>The formatted string.</returns>
        public static string ToStandard12HourDateTimeFormat(this DateTime dateTime)
        {
            return dateTime.ToString("dd MMMM yyyy, hh:mm:ss tt", LowercaseAmPmCulture);
        }

        /// <summary>
        /// Formats as a long date time format, e.g. "31 December 2024"
        /// </summary>
        /// <param name="date">The date to format.</param>
        /// <returns>The formatted string.</returns>
        public static string ToStandardDateFormat(this DateOnly date)
        {
            return date.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats as a short date format, e.g. "31/12/2024"
        /// </summary>
        /// <param name="date">The date to format.</param>
        /// <returns>The formatted string.</returns>
        public static string ToShortDateFormat(this DateOnly date)
        {
            return date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }


        private static CultureInfo CreateCulture()
        {
            var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            culture.DateTimeFormat.AMDesignator = "am";
            culture.DateTimeFormat.PMDesignator = "pm";

            return culture;
        }

        public static string ToDateDisplayFormat(this DateOnly date)
        {
            return date.ToString("dd MMMM yyyy");
        }
    }
}
