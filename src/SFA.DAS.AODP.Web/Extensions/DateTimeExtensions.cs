namespace SFA.DAS.AODP.Web.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToDateTimeDisplayFormat(this DateTime dateTime)
        {
            return $"{dateTime.ToString("dd MMMM yyyy")} at {dateTime.ToString("hh:mmtt").ToLower()}";
        }

        public static string ToDateDisplayFormat(this DateTime dateTime)
        {
            return dateTime.ToString("dd MMMM yyyy");
        }
    }
}
