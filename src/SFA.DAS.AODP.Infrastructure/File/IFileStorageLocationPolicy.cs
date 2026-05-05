using SFA.DAS.Aodp.Domain.Files;

namespace SFA.DAS.AODP.Infrastructure.File
{
    public interface IFileStorageLocationPolicy
    {
        FileStorageLocation Resolve(FileCategory category, FileContext? context);
    }

    public record FileContext(
        Guid ApplicationId,
        Guid? QuestionId,
        Guid? MessageId
    );

    public record FileStorageLocation(
        string Container,
        string BlobPath
    );

}
