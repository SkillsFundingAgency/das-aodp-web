namespace SFA.DAS.AODP.Models.Exceptions.FormValidation;

public class MultipleChoiceOptionException : QuestionValidationException
{
    public MultipleChoiceOptionException(Guid id, string title, string givenOption) 
        : base(id, title, $"Option passed to '{title}' - '{givenOption}' is not a valid option. ") { }
}
