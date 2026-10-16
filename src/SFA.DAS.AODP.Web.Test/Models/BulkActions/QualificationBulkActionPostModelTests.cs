using SFA.DAS.AODP.Web.Models.BulkActions;
using SFA.DAS.AODP.Web.Validators.Messages;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Tests.Models.BulkActions;

public class QualificationBulkActionPostModelTests
{
    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void No_Qualifications_Selected_Returns_Error()
    {
        var model = new QualificationBulkActionPostModel
        {
            SelectedQualificationIds = new List<Guid>()
        };

        var results = Validate(model);

        var error = Assert.Single(results);
        Assert.Equal(ValidationMessages.QualificationsBulkAction.NoQualificationsSelected, error.ErrorMessage);
        Assert.Contains(nameof(QualificationBulkActionPostModel.SelectedQualificationIds), error.MemberNames);
    }

    [Fact]
    public void Qualifications_Selected_Returns_No_Error()
    {
        var model = new QualificationBulkActionPostModel
        {
            SelectedQualificationIds = new List<Guid> { Guid.NewGuid() }
        };

        var results = Validate(model);

        Assert.Empty(results);
    }
}