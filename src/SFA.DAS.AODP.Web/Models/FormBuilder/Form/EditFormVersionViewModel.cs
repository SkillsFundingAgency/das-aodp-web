namespace SFA.DAS.AODP.Web.Models.FormBuilder.Form;
public class EditFormVersionViewModel
{
    public Guid Id { get; set; }
    public string? Version { get; set; }
    public string? Status { get; set; }

    public string Title { get; set; }
    public int Order { get; set; }
    public string Description { get; set; }
    public string? DescriptionHTML { get; set; }
    public AdditionalActions AdditionalFormActions { get; set; } = new AdditionalActions();

    public List<Section> Sections { get; set; } = new();


    public class Section
    {
        public Guid Id { get; set; }
        public int Order { get; set; }
        public string? Title { get; set; }
    }

    public class AdditionalActions
    {
        public Guid? UnPublish { get; set; }
        public Guid? Publish { get; set; }
        public Guid? MoveUp { get; set; }
        public Guid? MoveDown { get; set; }
        public bool UpdateDescriptionPreview { get; set; }
    }

    public static EditFormVersionViewModel Map(GetFormVersionByIdQueryResponse response)
    {
        var viewModel = new EditFormVersionViewModel();

        viewModel.Id = response.Id;
        viewModel.Title = response.Title;
        viewModel.Description = response.Description;
        viewModel.DescriptionHTML = response.DescriptionHTML;
        viewModel.Version = response.Version.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
        viewModel.Status = response.Status.ToString();
        viewModel.Order = response.Order;
        foreach (var section in response.Sections)
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
