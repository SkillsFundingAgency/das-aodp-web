using SFA.DAS.Aodp.Domain.Files;

namespace SFA.DAS.AODP.Infrastructure.File
{
    public sealed class AzureBlobLocationPolicy
    : IFileStorageLocationPolicy
    {

        public const string ApplicationFilesContainer = "files";
        public const string ImportsContainer = "importfilescontainer";
        public const string FundedQualificationsContainer = "funded-qualifications-import";

        public const string PldnsPrefix = "Pldns";
        public const string DefundingListPrefix = "DefundingList";
        public const string MessagesPrefix = "messages";


        public FileStorageLocation Resolve(
            FileCategory category,
            FileContext? context)
        {
            return category switch
            {
                // Question uploads
                // files/{applicationId}/{questionId}/{fileId}
                FileCategory.QuestionUpload =>
                    new FileStorageLocation(
                        ApplicationFilesContainer,
                        $"{context?.ApplicationId}/" +
                        $"{context?.QuestionId}/" +
                        $"{Guid.NewGuid()}"
                    ),

                // Message attachments
                // files/messages/{applicationId}/{messageId}/{fileId}
                FileCategory.MessageAttachment =>
                    new FileStorageLocation(
                        ApplicationFilesContainer,
                        $"{MessagesPrefix}/" +
                        $"{context?.ApplicationId}/" +
                        $"{context?.MessageId}/" +
                        $"{Guid.NewGuid()}"
                    ),

                // PLDNs imports
                // importfilescontainer/Pldns/{fileId}
                FileCategory.Pldns =>
                    new FileStorageLocation(
                        ImportsContainer,
                        $"{PldnsPrefix}/" +
                        $"{Guid.NewGuid()}"
                    ),

                // Defunding list imports
                // importfilescontainer/DefundingList/{fileId}
                FileCategory.DefundingList =>
                    new FileStorageLocation(
                        ImportsContainer,
                        $"{DefundingListPrefix}/" +
                        $"{Guid.NewGuid()}"
                    ),

                _ =>
                    throw new ArgumentOutOfRangeException(nameof(category))
            };
        }
    }
}
