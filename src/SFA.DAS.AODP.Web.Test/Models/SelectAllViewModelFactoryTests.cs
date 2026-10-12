
using SFA.DAS.AODP.Web.Models.BulkActions;

namespace SFA.DAS.AODP.Web.Tests.Models.BulkActions;

public class SelectAllViewModelFactoryTests
{

    private const string QualificationsController = "Qualifications";
    private const string IndexAction = "Index";
    private const string ReviewArea = "Review";

    [Fact]
    public void ForQualifications_Sets_Base_Properties()
    {
        var result = SelectAllViewModelFactory.ForQualifications(
            controllerName: QualificationsController);

        Assert.Equal(QualificationsController, result.Controller);
        Assert.Equal(IndexAction, result.Action);
        Assert.Equal(ReviewArea, result.Area);
    }
   
}