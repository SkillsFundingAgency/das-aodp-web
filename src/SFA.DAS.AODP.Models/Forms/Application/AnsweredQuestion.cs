using SFA.DAS.AODP.Models.Forms.FormSchema;

namespace SFA.DAS.AODP.Models.Forms.Application;

public class AnsweredQuestion
{
    public int Id { get; set; }
    public int QuestionSchemaId { get; set; }
    public int Index { get; set; }
    public AnsweredStatus AnsweredStatus { get; set; }
    public QuestionType Type { get; set; }
    public string? TextValue { get; set; }
    public int? IntegerValue { get; set; }
    public float? DecimalValue { get; set; }
    public DateTime? DateValue { get; set; }
    public string? MultipleChoiceValue { get; set; }
    public bool? BooleanValue { get; set; }
}
