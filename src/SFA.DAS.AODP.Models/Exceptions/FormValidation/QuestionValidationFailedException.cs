namespace SFA.DAS.AODP.Models.Exceptions.FormValidation;

public class QuestionValidationFailedException : QuestionValidationException
{
    public QuestionValidationFailedException(Guid id, string title, string message) : base(id, title, message) { }
}
