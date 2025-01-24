namespace SFA.DAS.AODP.Models.Forms.FormBuilder;

public class Section
{
    public Guid Id { get; set; }
    public Guid FormId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int? NextSectionId { get; set; }
    public List<Page> Pages { get; set; }

    public Section()
    {
        Pages = new List<Page>();
    }
}
