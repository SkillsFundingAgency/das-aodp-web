namespace SFA.DAS.AODP.Models.Forms.FormSchema;

public class FormSchema
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Version { get; set; }
    public bool Draft { get; set; }
    public List<SectionSchema> SectionSchemas { get; set; } = new List<SectionSchema>();
}
