namespace SFA.DAS.AODP.Web.Constants
{
    public static class FormBuilderValidationMessages
    {
        // File upload
        public const string TooManyFiles = "The number of files cannot be greater than {0}";

        // Options
        public const string OptionTextCannotBeEmpty = "Option text cannot be empty";
        public const string CannotRemoveOptionWithRoutes = "You cannot remove this option because it has associated routes.";

        // TextInput / TextArea
        public const string MinWordsMustBeGreaterThanZero = "Minimum number of words must be greater than 0";
        public const string MaxWordsMustBeGreaterThanOrEqualToMin = "Maximum number of words must be greater than or equal to minimum number of words";
        public const string MaxWordsMustBeGreaterThanZero = "Maximum number of words must be greater than 0";

        // NumberInput
        public const string MaxNumberMustBeGreaterThanOrEqualToMin = "Maximum value must be greater than or equal to minimum value";

        // DateInput
        public const string MaxDateMustBeLaterThanOrEqualToMin = "Maximum date must be later than or equal to minimum date";
    }
}
