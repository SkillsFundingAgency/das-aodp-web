using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.FormBuilder.Requests.Questions;

public class UpdateQuestionApiRequest : IPutApiRequest
{
    public Guid PageId { get; set; }
    public Guid FormVersionId { get; set; }
    public Guid SectionId { get; set; }
    public Guid QuestionId { get; set; }

    public string PutUrl => $"/api/forms/{FormVersionId}/sections/{SectionId}/Pages/{PageId}/Questions/{QuestionId}";

    public object Data { get; set; }

    public class Question
    {
        public string Title { get; set; }
        public string Hint { get; set; }
        public bool Required { get; set; }

        public TextInputOptions TextInput { get; set; }

        public class TextInputOptions
        {
            public int? MinLength { get; set; }
            public int? MaxLength { get; set; }
        }
    }
}