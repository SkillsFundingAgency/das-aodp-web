using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.AODP.Models.Forms.FormBuilder;
using SFA.DAS.AODP.Common.Extensions;

namespace SFA.DAS.AODP.Web.Models.Forms;

public class EditFormVersion
{
    public EditFormVersion()
    {
        Status = FormStatus.Draft.ToSelectList();
        Sections = new List<Section>();
    }

    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime Version { get; set; }
    public int SelectedStatus { get; set; }
    public SelectList Status { get; }
    public int Order { get; set; }
    public DateTime DateCreated { get; set; }

    public List<Section> Sections { get; set; }

    public class Section
    {
        public Section(Guid formVersionId)
        {
            FormVersionId = formVersionId;
        }
        public Guid Id { get; set; }
        public Guid FormVersionId { get; }
        public int Order { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}
