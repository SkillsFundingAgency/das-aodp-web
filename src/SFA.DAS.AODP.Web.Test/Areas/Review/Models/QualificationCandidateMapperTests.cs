using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Models.Rollover
{
    public class QualificationCandidateMapperTests
    {
        [Fact]
        public void Map_ReturnsEmptyStrings_WhenKeysMissing()
        {
            var row = new Dictionary<string, string>();

            var result = QualificationCandidateMapper.Map(row);

            Assert.Equal(string.Empty, result.QualificationNumber);
            Assert.Equal(string.Empty, result.QualificationName);
            Assert.Equal(string.Empty, result.AwardingOrganisation);
        }

        [Fact]
        public void Map_MapsAllFieldsCorrectly()
        {
            var row = new Dictionary<string, string>
            {
                { QualificationImportColumns.QualificationNumber, "QN123" },
                { QualificationImportColumns.QualificationName, "Test Qualification" },
                { QualificationImportColumns.AwardingOrganisation, "City & Guilds" }
            };

            var result = QualificationCandidateMapper.Map(row);

            Assert.Equal("QN123", result.QualificationNumber);
            Assert.Equal("Test Qualification", result.QualificationName);
            Assert.Equal("City & Guilds", result.AwardingOrganisation);
        }

        [Fact]
        public void Map_HandlesEmptyValues()
        {
            var row = new Dictionary<string, string>
            {
                { QualificationImportColumns.QualificationNumber, "" },
                { QualificationImportColumns.QualificationName, "" },
                { QualificationImportColumns.AwardingOrganisation, "" }
            };

            var result = QualificationCandidateMapper.Map(row);

            Assert.Equal(string.Empty, result.QualificationNumber);
            Assert.Equal(string.Empty, result.QualificationName);
            Assert.Equal(string.Empty, result.AwardingOrganisation);
        }
    }
}
