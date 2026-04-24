namespace SFA.DAS.AODP.Web.Areas.Apply.Storage
{
    public static class ApplicationMessageStoragePaths
    {
        public static string MessageRoot(Guid applicationId) =>
            $"messages/{applicationId}";

        public static string MessageFiles(Guid applicationId, Guid messageId) =>
            $"messages/{applicationId}/{messageId}";
    }
}
