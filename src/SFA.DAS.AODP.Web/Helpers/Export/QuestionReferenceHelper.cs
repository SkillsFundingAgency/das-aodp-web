namespace SFA.DAS.AODP.Web.Helpers.Export
{
    public static class QuestionReferenceHelper
    {
        public static string GetReference(
            int sectionOrder,
            int pageOrder,
            int questionOrder)
        {
            return $"{sectionOrder}.{pageOrder}.{questionOrder}";
        }
    }
}
