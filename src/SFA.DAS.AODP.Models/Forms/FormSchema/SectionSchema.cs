namespace SFA.DAS.AODP.Models.Forms.FormSchema;

public class SectionSchema
{
    public int Id { get; set; }
    public int FormId { get; set; }
    public int Index { get; set; }
    public List<PageSchema> PageSchemas { get; set; } = new List<PageSchema>();
}
