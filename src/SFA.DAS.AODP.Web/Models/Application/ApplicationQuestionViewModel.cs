using SFA.DAS.AODP.Models.Forms.FormSchema;

namespace SFA.DAS.AODP.Web.Models.Application
{
    public class ApplicationQuestionViewModel
    {
        public Guid OrganisationId { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }
        public Guid PageId { get; set; }
        public bool IsSubmitted { get; set; }

        public List<Question> Questions { get; set; }

        public class Question
        {
            public Guid Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public QuestionType Type { get; set; }
            public bool Required { get; set; }
            public string? Hint { get; set; } = string.Empty;
            public Answer Answer { get; set; }
        }

        public class Answer
        {
            public string? TextValue { get; set; }
            public int? IntegerValue { get; set; }
            public DateTime? DateValue { get; set; }
            public string? MultipleChoiceValue { get; set; }
            public bool? BooleanValue { get; set; }
        }
    }
}