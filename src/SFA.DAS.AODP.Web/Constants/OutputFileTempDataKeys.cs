namespace SFA.DAS.AODP.Web.Constants
{
    public static class OutputFileTempDataKeys
    {
        public const string Success = "OutputFileSuccess";
        public const string Failed = "OutputFileFailed";

        public static string SuccessMessage => $"{Success}:Message";
        public static string SuccessToken => $"{Success}:Token";
        public static string FailedMessage => $"{Failed}:Message";
    }
}
