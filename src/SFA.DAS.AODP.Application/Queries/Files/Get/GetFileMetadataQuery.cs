using MediatR;
using SFA.DAS.Aodp.Domain.Files;
using System.Diagnostics.CodeAnalysis;
namespace SFA.DAS.AODP.Application.Queries.Files.Get
{
    [ExcludeFromCodeCoverage]
    public class GetFileMetadataQuery : IRequest<BaseMediatrResponse<GetFileMetadataQueryResponse>>
    {
        public FileCategory FileCategory { get; init; }
        public Guid? FileId { get; init; }
        public Guid? ApplicationId { get; init; }
        public Guid? MessageId { get; init; }
        public Guid? QuestionId { get; init; }
    }
}
