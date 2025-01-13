using SFA.DAS.AODP.Models.Forms.Application;
using SFA.DAS.AODP.Models.Forms.FormSchema;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;

namespace SFA.DAS.AODP.Models.Forms.Validators;

public class DateValidator : IQuestionValidator
{
    public bool Required { get; set; }
    public DateTime? GreaterThan { get; set; }
    public DateTime? LessThan { get; set; }
    public DateTime? EqualTo { get; set; }
    public DateTime? NotEqualTo { get; set; }

    public DateSpan? GreaterThanTimeInFuture { get; set; }
    public DateSpan? LessThanTimeInFuture { get; set; }
    public DateSpan? GreaterThanTimeInPast{ get; set; }
    public DateSpan? LessThanTimeInPast { get; set; }

    public void Validate(QuestionSchema schema, AnsweredQuestion answeredQuestion)
    {
        if (Required && answeredQuestion.DateValue is null) 
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must not be blank. ");

        if (GreaterThan is not null && (answeredQuestion.DateValue is null || GreaterThan >= answeredQuestion.DateValue))
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must be greater than {GreaterThan}. ");

        if (LessThan is not null && (answeredQuestion.DateValue is not null && LessThan < answeredQuestion.DateValue))
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must be less than {LessThan}. ");

        if (EqualTo is not null && answeredQuestion.DateValue != EqualTo)
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must be equal to {EqualTo}. ");

        if (NotEqualTo is not null && answeredQuestion.DateValue == NotEqualTo)
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must not be equal to {NotEqualTo}. ");

        ValidateRelativeRestrictions(schema, answeredQuestion);
    }

    public void ValidateRelativeRestrictions(QuestionSchema schema, AnsweredQuestion answeredQuestion)
    {
        if (GreaterThanTimeInFuture is not null)
        {
            var future = GreaterThanTimeInFuture.Add(DateTime.Now.Date);
            if (answeredQuestion.DateValue is null || answeredQuestion.DateValue < future.Date)
                throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must be greater than {future}. ");
        }

        if (LessThanTimeInFuture is not null)
        {
            var future = LessThanTimeInFuture.Add(DateTime.Now.Date);
            if (answeredQuestion.DateValue is not null && answeredQuestion.DateValue > future.Date)
                throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must be greater than {future}. ");
        }

        if (GreaterThanTimeInPast is not null)
        {
            var past = GreaterThanTimeInPast.Subtract(DateTime.Now.Date);
            if (answeredQuestion.DateValue is null || answeredQuestion.DateValue < past.Date)
                throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must be greater than {past}. ");
        }

        if (LessThanTimeInPast is not null)
        {
            var past = LessThanTimeInPast.Subtract(DateTime.Now.Date);
            if (answeredQuestion.DateValue is not null && answeredQuestion.DateValue > past.Date)
                throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must be greater than {past}. ");
        }
    }
}

public record DateSpan(int Years, int Months, int Days)
{
    public DateTime Subtract(DateTime date) =>
        date.AddYears(0 - Years)
            .AddMonths(0 - Months)
            .AddDays(0 - Days);
    public DateTime Add(DateTime date) =>
        date.AddYears(Years)
            .AddMonths(Months)
            .AddDays(Days);
}
