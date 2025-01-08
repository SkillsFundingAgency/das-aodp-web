namespace SFA.DAS.AODP.Models.Exceptions.FormValidation;

public class QuestionValidationException : Exception
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public QuestionValidationException() : base() { }
    public QuestionValidationException(string message) : base(message) { }
    public QuestionValidationException(int id, string title) : base() 
    { 
        Id = id;
        Title = title;
    }
    public QuestionValidationException(int id, string title, string message) : base(message)
    {
        Id = id;
        Title = title;
    }

}
