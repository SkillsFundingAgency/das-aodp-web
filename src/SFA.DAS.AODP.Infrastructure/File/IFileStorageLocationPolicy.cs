using SFA.DAS.Aodp.Domain.Files;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Infrastructure.File
{

    public interface IFileStorageLocationPolicy
    {
        FileStorageLocation Resolve(FileCategory category, FileContext? context);
    }

    [ExcludeFromCodeCoverage]
    public record FileContext(
        Guid ApplicationId,
        Guid? QuestionId,
        Guid? MessageId
    );

    [ExcludeFromCodeCoverage]
    public record FileStorageLocation(
        string Container,
        string BlobPath
    );

}
