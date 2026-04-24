namespace SFA.DAS.AODP.Models.Settings
{
    public class BlobStorageSettings
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
        public string QuarantineContainerName { get; set; }   
        public string SafeContainerName { get; set; }     

    }
}
