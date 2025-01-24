namespace SFA.DAS.AODP.Models.Forms.FormBuilder;

public class ValidationRule
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }
    public string ErrorMessage { get; set; }
    public List<Question> Questions { get; set; }

    public ValidationRule()
    {
        Questions = new List<Question>();
    }
}
