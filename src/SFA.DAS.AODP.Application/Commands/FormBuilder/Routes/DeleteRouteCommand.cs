using MediatR;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Routes
{
    public class DeleteRouteCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
    {
        public Guid PageId { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }
        public Guid QuestionId { get; set; }
    }
}