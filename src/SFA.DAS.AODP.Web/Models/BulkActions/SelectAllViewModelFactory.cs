namespace SFA.DAS.AODP.Web.Models.BulkActions
{
    [ExcludeFromCodeCoverage]
    public static class SelectAllViewModelFactory
    {
        public static SelectAllCheckboxesViewModel ForQualifications(string controllerName)
        {
            return new SelectAllCheckboxesViewModel
            {
                Controller = controllerName,
                Action = "Index",
                Area = "Review"
            };
        }

        public static SelectAllCheckboxesViewModel ForApplications(string controllerName)
        {

            return new SelectAllCheckboxesViewModel
            {
                Controller = controllerName,
                Action = "Index",
                Area = "Review"
            };
        }
    }
}
