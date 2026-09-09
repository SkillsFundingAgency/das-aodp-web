using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Application.Commands.Files
{
    [ExcludeFromCodeCoverage]
    public class DeleteFileMetataCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
    {
        public Guid FileId { get; init; }
    }
}
