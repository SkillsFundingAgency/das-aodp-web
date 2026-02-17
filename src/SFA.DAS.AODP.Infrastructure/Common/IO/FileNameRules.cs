namespace SFA.DAS.AODP.Infrastructure.Common.IO
{
    public static class WindowsFileNameRules
    {
        public const int MaxFileNameLength = 255;

        public static readonly HashSet<string> ReservedBaseNames =
            new(StringComparer.OrdinalIgnoreCase)
            {
            "CON","PRN","AUX","NUL",
            "COM1","COM2","COM3","COM4","COM5","COM6","COM7","COM8","COM9",
            "LPT1","LPT2","LPT3","LPT4","LPT5","LPT6","LPT7","LPT8","LPT9"
            };
    }
}
