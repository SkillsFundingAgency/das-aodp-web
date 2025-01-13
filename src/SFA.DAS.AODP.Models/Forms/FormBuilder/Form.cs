namespace SFA.DAS.AODP.Models.Forms.FormBuilder;

public class Form
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public bool Published { get; set; }
    public string Key { get; set; }
    public string ApplicationTrackingTemplate { get; set; }
    public List<Section> Sections { get; set; }

    public Form()
    {
        Sections = new List<Section>();
    }
}