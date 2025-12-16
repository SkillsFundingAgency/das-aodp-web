namespace SFA.DAS.AODP.Models.Settings
{
    public class BlobStorageSettings
    {
        public string ConnectionString { get; set; }
        public string FileUploadContainerName { get; set; }

        public string ImportFilesContainerName { get; set; }
    }
}
