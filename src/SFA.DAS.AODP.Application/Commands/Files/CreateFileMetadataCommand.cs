using MediatR;
using SFA.DAS.Aodp.Domain.Files;
namespace SFA.DAS.AODP.Application.Commands.Files
{
    public class CreateFileMetadataCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
    {
        public FileCategory FileCategory { get; init; }
        public Guid? ApplicationId { get; init; }
        public Guid? MessageId { get; init; }
        public Guid? QuestionId { get; init; }
        public string FileName { get; init; } = string.Empty;
        public string ContentType { get; init; } = string.Empty;
        public string BlobContainer { get; init; } = string.Empty;
        public string BlobPath { get; init; } = string.Empty;
        public string UploadedBy { get; init; } = string.Empty;
    }
}
