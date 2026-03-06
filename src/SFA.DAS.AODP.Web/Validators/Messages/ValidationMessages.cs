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

        public static class QualificationsBulkAction
        {
            public const string StatusRequired = "Select a status.";
            public const string NoQualificationsSelected = "Select at least 1 qualification before applying actions.";
        }

        public static class ApplicationsBulkAction
        {
            public const string NoApplicationsSelected = "Select at least 1 application before applying actions.";
            public const string NoActionSelected = "Select an action.";
            public const string NoReviewerSelected = "Select a value for reviewer 1 or reviewer 2.";
        }


    }
}
