namespace SFA.DAS.AODP.Models.Forms.FormSchema;

public class PageSchema
{
    public int Id { get; set; }
    public int SectionId { get; set; }
    public int FormId { get; set; }
    public int Index { get; set; }
    public List<QuestionSchema> QuestionSchemas { get; set; } = new List<QuestionSchema>();
}
