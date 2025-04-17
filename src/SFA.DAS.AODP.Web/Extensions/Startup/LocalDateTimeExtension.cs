namespace SFA.DAS.AODP.Web.Extensions.Startup
{
    public static class LocalDateTimeExtension
    {
        public static DateTime ToLocalDateTime(this DateTime dateTime)
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            return TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.Utc, timeZone);
        }
    }
}
