namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

public record SelectQualificationTypesViewModel
{
    [MustNotBeEmpty(ErrorMessage = "Select the qualification types you want to rollover")]
    public List<QualificationType> SelectedTypes { get; set; } = [];

    public List<CheckboxItem> Types { get; private set; } = [];

    public SelectQualificationTypesViewModel Set(IEnumerable<QualificationType> rolloverTypes)
    {
        Types = rolloverTypes.Select(r => new CheckboxItem
        {
            LabelText = r.Name,
            Value = r.Id.ToString(),
            IsChecked = SelectedTypes.Contains(r)
        }).ToList();

        return this;
    }

    public SelectQualificationTypesViewModel MarkAllAsChecked()
    {
        Types.ForEach(o => o.SetChecked());
        return this;
    }
}