namespace SFA.DAS.AODP.Web.Models.FormBuilder.Form;

public class CreateFormVersionViewModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string? DescriptionPreview { get; set; }
    public AdditionalActions AdditionalFormActions { get; set; } = new AdditionalActions();

    public class AdditionalActions
    {
        public bool UpdateDescriptionPreview { get; set; }
    }
}
