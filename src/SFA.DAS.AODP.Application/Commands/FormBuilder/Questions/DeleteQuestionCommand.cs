using MediatR;
namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;
public class DeleteQuestionCommand : IRequest<BaseMediatrResponse<DeleteQuestionCommandResponse>>
{
    public Guid QuestionId { get; set; }
    public Guid FormVersionId { get; set; }
    public Guid SectionId { get; set; }
    public Guid PageId { get; set; }
}