using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Web.Models.FormBuilder.Form;

public class CreateFormVersionViewModel
{
    //public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime Version { get; set; }
    public int SelectedStatus { get; set; }
    public FormStatus Status { get; set; }
    public int Order { get; set; }
    public DateTime DateCreated { get; set; }
}
