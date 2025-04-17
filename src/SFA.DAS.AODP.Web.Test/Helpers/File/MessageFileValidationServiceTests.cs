using Microsoft.AspNetCore.Http;
using Moq;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Helpers.File;

namespace SFA.DAS.AODP.Web.UnitTests.Helpers.File
{
    public class MessageFileValidationServiceTests
    {
        private readonly FormBuilderSettings _formBuilderSettings = new();
        private readonly MessageFileValidationService _sut;

        public MessageFileValidationServiceTests() => _sut = new(_formBuilderSettings);

        [Fact]
        public void ValidateFiles_TooManyFiles_ErrorThrown()
        {
            // Arrange
            _formBuilderSettings.MaxUploadNumberOfFiles = 1;
            List<IFormFile> files = [Mock.Of<IFormFile>(), Mock.Of<IFormFile>()];

            // Act
            var ex = Assert.Throws<Exception>(() => _sut.ValidateFiles(files));

            // Assert
            Assert.Equal($"Cannot upload more than {_formBuilderSettings.MaxUploadNumberOfFiles} files", ex.Message);
        }

        [Fact]
        public void ValidateFiles_FileTooBig_ErrorThrown()
        {
            // Arrange
            _formBuilderSettings.MaxUploadNumberOfFiles = 1;
            _formBuilderSettings.MaxUploadFileSize = 1;
            Mock<IFormFile> file = new();
            List<IFormFile> files = [file.Object];

            file.SetupGet(f => f.Length).Returns(2 * 1024 * 1024);

            // Act
            var ex = Assert.Throws<Exception>(() => _sut.ValidateFiles(files));

            // Assert
            Assert.Equal($"File size exceeds max allowed size of {_formBuilderSettings.MaxUploadFileSize}mb.", ex.Message);
        }

        [Fact]
        public void ValidateFiles_FileWrongExtension_ErrorThrown()
        {
            // Arrange
            _formBuilderSettings.MaxUploadNumberOfFiles = 1;
            _formBuilderSettings.MaxUploadFileSize = 1;
            _formBuilderSettings.UploadFileTypesAllowed = [".DOCX"];

            Mock<IFormFile> file = new();
            List<IFormFile> files = [file.Object];

            file.SetupGet(f => f.Length).Returns(1);
            file.SetupGet(f => f.FileName).Returns("Test.pdf"); 

            // Act
            var ex = Assert.Throws<Exception>(() => _sut.ValidateFiles(files));

            // Assert
            Assert.Equal($"File type is not included in the allowed file types: {string.Join(",", _formBuilderSettings.UploadFileTypesAllowed)}", ex.Message);
        }

        [Fact]
        public void ValidateFiles_FileValid_NoErrorThrown()
        {
            // Arrange
            _formBuilderSettings.MaxUploadNumberOfFiles = 1;
            _formBuilderSettings.MaxUploadFileSize = 1;
            _formBuilderSettings.UploadFileTypesAllowed = [".DOCX"];

            Mock<IFormFile> file = new();
            List<IFormFile> files = [file.Object];

            file.SetupGet(f => f.Length).Returns(1);
            file.SetupGet(f => f.FileName).Returns("Test.docx");

            // Act
            _sut.ValidateFiles(files);

        }
    }
}
