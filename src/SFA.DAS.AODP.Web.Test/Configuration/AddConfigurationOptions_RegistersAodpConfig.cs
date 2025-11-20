using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Extensions.Startup;

namespace SFA.DAS.AODP.Web.UnitTests.Configuration
{
    public class AddConfigurationOptions_RegistersAodpConfigTests
    {
        [Fact]
        public void AddConfigurationOptions_RegistersAodpOuterApiSettings()
        {
            // Arrange
            var inmemorySettings = new Dictionary<string, string>
            {
                { "AodpOuterApiSettings:Key", "TestKey" },
                { "AodpOuterApiSettings:BaseUrl", "https://localhost:7069/" }
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inmemorySettings)
                .Build();

            var services = new ServiceCollection();

            // Act
            services.AddConfigurationOptions(configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Assert 
            var options = serviceProvider.GetRequiredService<IOptions<AodpOuterApiSettings>>();
            Assert.Equal("TestKey", options.Value.Key);
            Assert.Equal("https://localhost:7069/", options.Value.BaseUrl);

            var singleton = serviceProvider.GetRequiredService<AodpOuterApiSettings>();
            Assert.Equal("TestKey", singleton.Key);
            Assert.Equal("https://localhost:7069/", singleton.BaseUrl);
        }

        [Fact]
        public void AddConfigurationOptions_RegistersFormBuilderSettings()
        {
            //Arrange
            var inMemorySettings = new Dictionary<string, string>
            {
                { "FormBuilderSettings:UploadFileTypesAllowed:0",".PDF" },
                { "FormBuilderSettings:UploadFileTypesAllowed:1",".DOCX" },
                { "FormBuilderSettings:MaxUploadFileSize", "10" },
                { "FormBuilderSettings:MaxUploadNumberOfFiles", "10" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var services = new ServiceCollection();
            services.AddConfigurationOptions(configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetRequiredService<IOptions<FormBuilderSettings>>();
            var singleton = serviceProvider.GetRequiredService<FormBuilderSettings>();

            // Assert
            Assert.Equal(2, options.Value.UploadFileTypesAllowed.Count);
            Assert.Contains(".PDF", options.Value.UploadFileTypesAllowed);
            Assert.Contains(".DOCX", options.Value.UploadFileTypesAllowed);

            Assert.Equal(10, options.Value.MaxUploadFileSize);
            Assert.Equal(10, options.Value.MaxUploadNumberOfFiles);

            Assert.Equal(2, singleton.UploadFileTypesAllowed.Count);
            Assert.Contains(".PDF", singleton.UploadFileTypesAllowed);
            Assert.Contains(".DOCX", singleton.UploadFileTypesAllowed);
            Assert.Equal(10, singleton.MaxUploadFileSize);
            Assert.Equal(10, singleton.MaxUploadNumberOfFiles);
        }

        [Fact]
        public void AddConfigurationOptions_RegistersBlobStorageSettings()
        {
            // Arrange
            var inmemorySettings = new Dictionary<string, string>
            {
                { "BlobStorageSettings:ConnectionString", "ConnectionString" },
                { "BlobStorageSettings:FileUploadContainerName", "containerName" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inmemorySettings)
                .Build();
            var services = new ServiceCollection();

            // Act
            services.AddConfigurationOptions(configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Assert 
            var options = serviceProvider.GetRequiredService<IOptions<BlobStorageSettings>>();
            Assert.Equal("ConnectionString", options.Value.ConnectionString);
            Assert.Equal("containerName", options.Value.FileUploadContainerName);

            var singleton = serviceProvider.GetRequiredService<BlobStorageSettings>();
            Assert.Equal("ConnectionString", singleton.ConnectionString);
            Assert.Equal("containerName", singleton.FileUploadContainerName);
        }

        [Fact]
        public void AddConfigurationOptions_RegistersFindRegulatedQualificationUrl()
        {
            // Arrange
            var inmemorySettings = new Dictionary<string, string>
            {
                { "FindRegulatedQualificationUrl", "ConfigSetUrl" }
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inmemorySettings)
                .Build();

            var services = new ServiceCollection();

            // Act
            services.AddConfigurationOptions(configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Assert 
            var options = serviceProvider.GetRequiredService<IOptions<AodpConfiguration>>();
            Assert.Equal("ConfigSetUrl", options.Value.FindRegulatedQualificationUrl);
            var singleton = serviceProvider.GetRequiredService<AodpConfiguration>();
            Assert.Equal("ConfigSetUrl", singleton.FindRegulatedQualificationUrl);
        }
    }
}
