using Azure;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;

namespace SFA.DAS.AODP.Web.Models.Forms;

public class EditFormVersionViewModel
{

    public Guid Id { get; set; }
    public string Version { get; set; }
    public string Status { get; set; }

    public string? Name { get; set; }
    public int Order { get; set; }
    public string? Description { get; set; }


    public List<Section> Sections { get; set; } = new();

    public class Section
    {
        public Guid Id { get; set; }
        public int Order { get; set; }
        public string? Title { get; set; }
    }

    public static EditFormVersionViewModel Map(GetFormVersionByIdQueryResponse response)
    {
        var viewModel = new EditFormVersionViewModel();

        viewModel.Id = response.Data.Id;
        viewModel.Name = response.Data.Name;
        viewModel.Description = response.Data.Description;
        viewModel.Version = response.Data.Version.ToString("yyyy-MM-dd HH:mm");
        viewModel.Status = response.Data.Status.ToString();
        viewModel.Order = response.Data.Order;
        foreach (var section in response.Data.Sections)
        {
            //Link sections to formVersionId
            var sectionItem = new Section();

            sectionItem.Id = section.Id;
            sectionItem.Order = section.Order;
            sectionItem.Title = section.Title;

            viewModel.Sections.Add(sectionItem);
        }

        return viewModel;
    }
}
