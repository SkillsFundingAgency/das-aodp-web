namespace SFA.DAS.ADPO.Models.Configuration
{
    public class AzureTableStorageConfiguration
    {
        public string ConfigurationKeys { get; set; }
        public string StorageConnectionString { get; set; }
        public string EnvironmentName { get; set; }
        public string Version { get; set; }
    }
}
