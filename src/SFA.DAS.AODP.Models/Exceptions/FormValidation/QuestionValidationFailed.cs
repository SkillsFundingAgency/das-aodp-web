namespace SFA.DAS.AODP.Models.Exceptions.FormValidation;

public class QuestionValidationFailed : QuestionValidationException
{
    public QuestionValidationFailed(Guid id, string title, string message) : base(id, title, message) { }
}
