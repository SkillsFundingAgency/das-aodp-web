namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

public record SelectQualificationLevelsViewModel
{
    [BindProperty]
    [MustNotBeEmpty(ErrorMessage = "Select the qualification levels you want to rollover")]
    public List<QualificationLevel> SelectedLevels { get; set; } = [];

    public List<CheckboxItem> Levels { get; private set; } = [];

    public SelectQualificationLevelsViewModel SetLevels(IEnumerable<QualificationLevel> rolloverLevels)
    {
        Levels = rolloverLevels.Select(r => new CheckboxItem
        {
            LabelText = r.Name,
            Value = r.Id.ToString(),
            IsChecked = SelectedLevels.Contains(r)
        }).ToList();
        return this;
    }

    public SelectQualificationLevelsViewModel MarkAllAsChecked()
    {
        Levels.ForEach(o => o.SetChecked());
        return this;
    }
}