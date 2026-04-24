namespace SFA.DAS.AODP.Web.Areas.Apply.Storage
{
    
    public static class ApplicationStoragePaths
    {
        public static string ApplicationRoot(Guid applicationId) =>
            $"applications/{applicationId}";

        public static string QuestionFiles(Guid applicationId, Guid questionId) =>
            $"applications/{applicationId}/questions/{questionId}";
    }

}
