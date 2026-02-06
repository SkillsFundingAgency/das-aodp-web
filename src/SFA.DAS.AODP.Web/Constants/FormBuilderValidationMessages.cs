namespace SFA.DAS.AODP.Web.Constants
{
    public static class FormBuilderValidationMessages
    {
        // File upload
        public const string TooManyFiles = "The number of files cannot be greater than {0}";
        public const string MandatoryFileUploadMaxFiles = "You cannot set a maximum limit of 0 for a mandatory file upload.";

        // Options
        public const string OptionTextCannotBeEmpty = "Option text cannot be empty";
        public const string CannotRemoveOptionWithRoutes = "You cannot remove this option because it has associated routes.";

        // TextInput / TextArea
        public const string MinWordsMustBeGreaterThanZero = "The minimum number of words must be greater than 0.";
        public const string MaxWordsMustBeGreaterThanOrEqualToMin = "The minimum number of words must be lower than or equal to the maximum.";
        public const string MaxWordsMustBeGreaterThanZero = "The maximum number of words must be greater than 0.";

        // NumberInput
        public const string MaxNumberMustBeGreaterThanOrEqualToMin = "The minimum value cannot exceed the maximum value.";

        // DateInput
        public const string MaxDateMustBeLaterThanOrEqualToMin = "The minimum date cannot be after the maximum date.";

        //MultiChoice
        public const string MinOptionsCannotBeGreaterThanMaxOptions = "The minimum number of options cannot be higher than the maximum.";
        public const string MinOptionsCannotBeGreaterThanAvailableOptions = "Add more options or edit the minimum number required.";
    }
}
