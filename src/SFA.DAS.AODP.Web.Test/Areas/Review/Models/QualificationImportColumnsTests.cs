using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Models
{
    public class QualificationImportColumnsTests
    {
        [Fact]
        public void ToCommaSeparatedWithAnd_ReturnsExpectedString()
        {
            var result = QualificationImportColumns.ColumnNamesForView();

            Assert.Equal(
                "QualificationName, AwardingOrganisation, QualificationNumber, Level, QualificationType, Subcategory, SectorSubjectArea and Status",
                result);
        }
    }
}
