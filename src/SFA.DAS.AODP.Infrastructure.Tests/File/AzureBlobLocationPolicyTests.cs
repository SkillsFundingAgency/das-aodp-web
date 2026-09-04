using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.Aodp.Domain.Files;
using Xunit;

namespace SFA.DAS.AODP.Infrastructure.UnitTests.File
{
    public class AzureBlobLocationPolicyTests
    {
        private readonly AzureBlobLocationPolicy _sut;

        public AzureBlobLocationPolicyTests()
        {
            _sut = new AzureBlobLocationPolicy();
        }

        [Fact]
        public void Resolve_QuestionUpload_Returns_ApplicationFilesContainer_With_Application_And_Question_Path()
        {
            // Arrange
            var applicationId = Guid.NewGuid();
            var questionId = Guid.NewGuid();

            var context = new FileContext(
                ApplicationId: applicationId,
                QuestionId: questionId,
                MessageId: null);

            // Act
            var result = _sut.Resolve(FileCategory.QuestionUpload, context);

            // Assert
            Assert.Equal(AzureBlobLocationPolicy.ApplicationFilesContainer, result.Container);
            Assert.StartsWith(
                $"{applicationId}/{questionId}/",
                result.BlobPath);
        }

        [Fact]
        public void Resolve_MessageAttachment_Returns_ApplicationFilesContainer_With_Message_Path()
        {
            // Arrange
            var applicationId = Guid.NewGuid();
            var messageId = Guid.NewGuid();

            var context = new FileContext(
                ApplicationId: applicationId,
                QuestionId: null,
                MessageId: messageId);

            // Act
            var result = _sut.Resolve(FileCategory.MessageAttachment, context);

            // Assert
            Assert.Equal(AzureBlobLocationPolicy.ApplicationFilesContainer, result.Container);
            Assert.StartsWith(
                $"{AzureBlobLocationPolicy.MessagesPrefix}/{applicationId}/{messageId}/",
                result.BlobPath);
        }

        [Fact]
        public void Resolve_Pldns_Returns_ImportsContainer_With_Fixed_Path()
        {
            // Act
            var result = _sut.Resolve(FileCategory.Pldns, context: null);

            // Assert
            Assert.Equal(AzureBlobLocationPolicy.ImportsContainer, result.Container);
            Assert.Equal(
                $"{AzureBlobLocationPolicy.PldnsPrefix}/{AzureBlobLocationPolicy.PldnsPrefix}.xlsx",
                result.BlobPath);
        }

        [Fact]
        public void Resolve_DefundingList_Returns_ImportsContainer_With_Fixed_Path()
        {
            // Act
            var result = _sut.Resolve(FileCategory.DefundingList, context: null);

            // Assert
            Assert.Equal(AzureBlobLocationPolicy.ImportsContainer, result.Container);
            Assert.Equal(
                $"{AzureBlobLocationPolicy.DefundingListPrefix}/{AzureBlobLocationPolicy.DefundingListPrefix}.xlsx",
                result.BlobPath);
        }

        [Fact]
        public void Resolve_Pldns_And_DefundingList_Return_The_Same_BlobPath_On_Repeated_Calls()
        {
            // These categories represent a single, ongoing import file that gets overwritten
            // on each upload — unlike QuestionUpload/MessageAttachment, repeated calls must
            // resolve to the exact same location, not a fresh one each time.

            // Act
            var pldnsFirst = _sut.Resolve(FileCategory.Pldns, null);
            var pldnsSecond = _sut.Resolve(FileCategory.Pldns, null);

            var defundingFirst = _sut.Resolve(FileCategory.DefundingList, null);
            var defundingSecond = _sut.Resolve(FileCategory.DefundingList, null);

            // Assert
            Assert.Equal(pldnsFirst.BlobPath, pldnsSecond.BlobPath);
            Assert.Equal(defundingFirst.BlobPath, defundingSecond.BlobPath);
        }

        [Fact]
        public void Resolve_QuestionUpload_Generates_Unique_BlobPaths()
        {
            // Unlike the import categories, question uploads/message attachments must remain
            // distinct even when they otherwise share the same context.
            var applicationId = Guid.NewGuid();
            var questionId = Guid.NewGuid();
            var context = new FileContext(ApplicationId: applicationId, QuestionId: questionId, MessageId: null);

            // Act
            var first = _sut.Resolve(FileCategory.QuestionUpload, context);
            var second = _sut.Resolve(FileCategory.QuestionUpload, context);

            // Assert
            Assert.NotEqual(first.BlobPath, second.BlobPath);
        }

        [Fact]
        public void Resolve_When_Category_Is_Unsupported_Throws_ArgumentOutOfRangeException()
        {
            // Arrange
            var invalidCategory = (FileCategory)999;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _sut.Resolve(invalidCategory, null));
        }
    }
}