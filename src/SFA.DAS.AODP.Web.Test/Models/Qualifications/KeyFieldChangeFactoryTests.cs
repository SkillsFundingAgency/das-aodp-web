using SFA.DAS.AODP.Web.Extensions;
using SFA.DAS.AODP.Web.Models.Qualifications;

namespace SFA.DAS.AODP.Web.UnitTests.Models.Qualifications;

public class KeyFieldChangeFactoryTests : UnitTest
{
    [Fact]
    public void Create_ReturnsNull_WhenAnyParameterIsNull()
    {
        // Arrange
        ChangedQualificationDetailsViewModel latest = new();
        ChangedQualificationDetailsViewModel previous = new();
        KeyField? nullKey = null;

        // Act
        var resultForNullKey = KeyFieldChangeFactory.Create(nullKey!, latest, previous);
        var resultForNullLatest = KeyFieldChangeFactory.Create(KeyField.Level, null!, previous);
        var resultForNullPrevious = KeyFieldChangeFactory.Create(KeyField.Level, latest, null!);

        // Assert
        resultForNullKey.ShouldBeNull();
        resultForNullLatest.ShouldBeNull();
        resultForNullPrevious.ShouldBeNull();
    }

    [Fact]
    public void Create_ReturnsOrganisationNameChange()
    {
        // Arrange
        var previous = new ChangedQualificationDetailsViewModel
        {
            Organisation = new ChangedQualificationDetailsViewModel.AwardingOrganisation { NameOfqual = "PrevOrg" }
        };

        var latest = new ChangedQualificationDetailsViewModel
        {
            Organisation = new ChangedQualificationDetailsViewModel.AwardingOrganisation { NameOfqual = "NewOrg" }
        };

        // Act
        var result = KeyFieldChangeFactory.Create(KeyField.OrganisationName, latest, previous);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(KeyField.OrganisationName.DisplayName);
        result.Was.ShouldBe("PrevOrg");
        result.Now.ShouldBe("NewOrg");
    }

    [Fact]
    public void Create_ReturnsLevelChange()
    {
        // Arrange
        var previous = new ChangedQualificationDetailsViewModel { Level = "1" };
        var latest = new ChangedQualificationDetailsViewModel { Level = "2" };

        // Act
        var result = KeyFieldChangeFactory.Create(KeyField.Level, latest, previous);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(KeyField.Level.DisplayName);
        result.Was.ShouldBe("1");
        result.Now.ShouldBe("2");
    }

    [Fact]
    public void Create_FormatsOperationalEndDate()
    {
        // Arrange
        var previousDate = new DateTime(2020, 1, 2, 13, 30, 0);
        var latestDate = new DateTime(2021, 6, 10, 9, 15, 0);

        var previous = new ChangedQualificationDetailsViewModel { OperationalEndDate = previousDate };
        var latest = new ChangedQualificationDetailsViewModel { OperationalEndDate = latestDate };

        // Act
        var result = KeyFieldChangeFactory.Create(KeyField.OperationalEndDate, latest, previous);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(KeyField.OperationalEndDate.DisplayName);
        result.Was.ShouldBe(previousDate.ToStandard12HourDateTimeFormat());
        result.Now.ShouldBe(latestDate.ToStandard12HourDateTimeFormat());
    }

    [Fact]
    public void Create_HandlesAllDefinedKeyFields_ReturnsKeyFieldChanges()
    {
        // Arrange
        var previous = new ChangedQualificationDetailsViewModel
        {
            Organisation = new ChangedQualificationDetailsViewModel.AwardingOrganisation { NameOfqual = "PrevOrg" }
        };

        var latest = new ChangedQualificationDetailsViewModel
        {
            Organisation = new ChangedQualificationDetailsViewModel.AwardingOrganisation { NameOfqual = "NewOrg" }
        };

        // Act & Assert
        foreach (var key in KeyField.All)
        {
            var result = KeyFieldChangeFactory.Create(key, latest, previous);
            result.ShouldNotBeNull();
            result.Name.ShouldBe(key.DisplayName);
        }
    }
}
