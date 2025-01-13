namespace SFA.DAS.AODP.Models.Forms.FormBuilder;

public class Question
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    public bool Required { get; set; }
    public int Order { get; set; }
    public string Description { get; set; }
    public string Hint { get; set; }
    public List<Option> Options { get; set; }
    public List<ValidationRule> ValidationRules { get; set; }

    public Question()
    {
        Options = new List<Option>();
        ValidationRules = new List<ValidationRule>();
    }
}
