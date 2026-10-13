namespace SFA.DAS.AODP.Web.Validators.Patterns
{
    public static class ValidationPatterns
    {
        // Structural - must match this shape
        public static class Format
        {
            public const string QualificationNumber =
                @"^(?:\d{8}|\d{7}[A-Za-z]|[0-9]{3}/[0-9]{4}/[0-9A-Za-z])$";
        }

        // Text - allowed characters
        public static class Text
        {
            public const string Title =
                @"^[A-Za-z0-9 \-'\.&/(),:;]+$";

            public const string PersonName =
                @"^[A-Za-z \-']+$";

        }
    }
}