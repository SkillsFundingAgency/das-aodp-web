using SFA.DAS.Aodp.Domain.Files;

namespace SFA.DAS.AODP.Infrastructure.File
{
    public sealed class AzureBlobLocationPolicy
    : IFileStorageLocationPolicy
    {

        public const string ApplicationFilesContainer = "files";
        public const string ImportsContainer = "importfilescontainer";
        public const string FundedQualificationsContainer = "funded-qualifications-import";
        public const string RolloverCandidateImportContainer = "rollover-import";
        public const string RolloverCandidateSubmittedContainer = "rollover-submitted";

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
                // importfilescontainer/Pldns/Pldns.xlsx
                // Overwritten on each import — there is only ever one current PLDNS file,
                // tracked by a single FileRecord per FileCategory.Pldns.
                FileCategory.Pldns =>
                    new FileStorageLocation(
                        ImportsContainer,
                        $"{PldnsPrefix}/{PldnsPrefix}.xlsx"
                    ),

                // Defunding list imports
                // importfilescontainer/DefundingList/DefundingList.xlsx
                // Overwritten on each import — there is only ever one current file,
                // tracked by a single FileRecord per FileCategory.DefundingList.
                FileCategory.DefundingList =>
                    new FileStorageLocation(
                        ImportsContainer,
                        $"{DefundingListPrefix}/{DefundingListPrefix}.xlsx"
                    ),

                // Rollover candidate list imports
                // rollover-import/{fileId}.csv
                FileCategory.RolloverCandidateImport =>
                    new FileStorageLocation(
                        RolloverCandidateImportContainer,
                        $"{Guid.NewGuid()}.csv"
                    ),

                // Rollover final-list (submitted for rollover)
                // rollover-submitted/{fileId}.csv
                FileCategory.RolloverCandidateSubmitted =>
                    new FileStorageLocation(
                        RolloverCandidateSubmittedContainer,
                        $"{Guid.NewGuid()}.csv"
                    ),

                _ =>
                    throw new ArgumentOutOfRangeException(nameof(category))
            };
        }
    }
}
