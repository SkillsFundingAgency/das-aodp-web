using SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;
using static SFA.DAS.AODP.Application.Queries.FormBuilder.Questions.GetQuestionByIdQueryResponse;
namespace SFA.DAS.AODP.Web.Models.FormBuilder.Question;

public class DeleteQuestionViewModel
{
    public Guid QuestionId { get; set; }
    public Guid PageId { get; set; }
    public Guid SectionId { get; set; }
    public Guid FormVersionId { get; set; }
    public string Title { get; set; }
    public List<RouteInformation> Routes { get; set; } = new();

    public static DeleteQuestionViewModel MapToViewModel(GetQuestionByIdQueryResponse response, Guid formVersionId, Guid sectionId)
    {
        return new DeleteQuestionViewModel()
        {
            QuestionId = response.Id,
            PageId = response.PageId,
            SectionId = sectionId,
            FormVersionId = formVersionId,
            Title = response.Title,
            Routes = response.Routes
        };
    }
}