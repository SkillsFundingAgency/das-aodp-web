using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Models.BulkActions
{
    [ExcludeFromCodeCoverage]
    public sealed record MultiSelectCheckboxCellViewModel
    (
        Guid Id,
        string Title,
        bool IsChecked,
        string SelectedIdsFieldName, 
        string IdPrefix = "sel"
    );
}
