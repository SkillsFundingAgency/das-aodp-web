namespace SFA.DAS.AODP.Domain.FormBuilder.Responses.Questions;

public class GetQuestionByIdApiResponse() 
{
    public Question Data { get; set; }

    public class Question
    {
        public Guid Id { get; set; }
        public Guid PageId { get; set; }
        public string Title { get; set; }
        public Guid Key { get; set; }
        public string Hint { get; set; }
        public int Order { get; set; }
        public bool Required { get; set; }
        public string Type { get; set; }
    }
}