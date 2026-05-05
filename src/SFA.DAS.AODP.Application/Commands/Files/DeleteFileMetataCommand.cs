using MediatR;

namespace SFA.DAS.AODP.Application.Commands.Files
{
    public class DeleteFileMetataCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
    {
        public Guid FileId { get; init; }
    }
}
