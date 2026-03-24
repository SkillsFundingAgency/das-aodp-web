namespace SFA.DAS.AODP.Web.Validators.Messages
{
    public static class ValidationMessages
    {
        public const string QualificationTitleRequired = "Enter a qualification title.";
        public const string QualificationTitleTooLong = "Qualification title must be 200 characters or fewer.";
        public const string QualificationTitleInvalidChars = "Qualification title contains invalid characters.";

        public const string ApplicationOwnerRequired = "Enter an application owner.";
        public const string ApplicationOwnerTooLong = "Application owner must be 200 characters or fewer.";
        public const string ApplicationOwnerInvalidChars = "Application owner contains invalid characters.";

        public const string QualificationsBulkActionStatusRequired = "Select a status.";
        public const string QualificationsBulkActionNoQualificationsSelected = "Select at least 1 qualification before applying actions.";
    }
}
