using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Models.Rollover
{
    public class FundingExtensionCandidateMapperTests
    {
        [Fact]
        public void Map_ReturnsEmptyStrings_AndNulls_WhenKeysMissing()
        {
            var row = new Dictionary<string, string>();

            var result = FundingExtensionCandidateMapper.Map(row);

            Assert.Equal(string.Empty, result.Qan);
            Assert.Null(result.OperationalEndDate);
            Assert.Null(result.OfferedInEngland);
            Assert.Null(result.FundedInEngland);
        }

        [Fact]
        public void Map_ParsesValidDates()
        {
            var row = new Dictionary<string, string>
            {
                { FundingExtensionCandidateColumns.OperationalEndDate, "2024-01-01" }
            };

            var result = FundingExtensionCandidateMapper.Map(row);

            Assert.Equal(new DateTime(2024, 1, 1), result.OperationalEndDate);
        }

        [Fact]
        public void Map_ReturnsNull_ForInvalidDates()
        {
            var row = new Dictionary<string, string>
            {
                { FundingExtensionCandidateColumns.OperationalEndDate, "not-a-date" }
            };

            var result = FundingExtensionCandidateMapper.Map(row);

            Assert.Null(result.OperationalEndDate);
        }

        [Fact]
        public void Map_ParsesValidBooleans()
        {
            var row = new Dictionary<string, string>
            {
                { FundingExtensionCandidateColumns.OfferedInEngland, "TRUE" },
                { FundingExtensionCandidateColumns.FundedInEngland, "FALSE" }
            };

            var result = FundingExtensionCandidateMapper.Map(row);

            Assert.True(result.OfferedInEngland);
            Assert.False(result.FundedInEngland);
        }

        [Fact]
        public void Map_ReturnsNull_ForInvalidBooleans()
        {
            var row = new Dictionary<string, string>
            {
                { FundingExtensionCandidateColumns.OfferedInEngland, "yes" },
                { FundingExtensionCandidateColumns.FundedInEngland, "nope" }
            };

            var result = FundingExtensionCandidateMapper.Map(row);

            Assert.Null(result.OfferedInEngland);
            Assert.Null(result.FundedInEngland);
        }

        [Fact]
        public void Map_MapsAllFieldsCorrectly()
        {
            var row = new Dictionary<string, string>
            {
                { FundingExtensionCandidateColumns.Qan, "123" },
                { FundingExtensionCandidateColumns.QualificationTitle, "Title" },
                { FundingExtensionCandidateColumns.AwardingOrganisation, "Org" },
                { FundingExtensionCandidateColumns.QualificationLevel, "3" },
                { FundingExtensionCandidateColumns.QualificationType, "Type" },
                { FundingExtensionCandidateColumns.Ssa, "SSA" },
                { FundingExtensionCandidateColumns.OperationalEndDate, "2024-01-01" },
                { FundingExtensionCandidateColumns.OfferedInEngland, "true" },
                { FundingExtensionCandidateColumns.FundedInEngland, "false" },
                { FundingExtensionCandidateColumns.Glh, "100" },
                { FundingExtensionCandidateColumns.Tqt, "200" },
                { FundingExtensionCandidateColumns.Pre16, "true" },
                { FundingExtensionCandidateColumns.Age16To18, "false" },
                { FundingExtensionCandidateColumns.Age18Plus, "true" },
                { FundingExtensionCandidateColumns.Age19Plus, "false" },
                { FundingExtensionCandidateColumns.FundingStreamName, "FS1" },
                { FundingExtensionCandidateColumns.FundingApprovalStartDate, "2023-09-01" },
                { FundingExtensionCandidateColumns.ProposedOutcome, "Extend" },
                { FundingExtensionCandidateColumns.RollOverStatus, "Extend" },
                { FundingExtensionCandidateColumns.ExclusionReason, "None" },
                { FundingExtensionCandidateColumns.CurrentFundingApprovalEndDate, "2024-07-31" },
                { FundingExtensionCandidateColumns.ProposedFundingApprovalEndDate, "2025-07-31" },
                { FundingExtensionCandidateColumns.Comments, "Some comments" }
            };

            var result = FundingExtensionCandidateMapper.Map(row);

            Assert.Equal("123", result.Qan);
            Assert.Equal("Title", result.QualificationTitle);
            Assert.Equal("Org", result.AwardingOrganisation);
            Assert.Equal("3", result.QualificationLevel);
            Assert.Equal("Type", result.QualificationType);
            Assert.Equal("SSA", result.Ssa);
            Assert.Equal(new DateTime(2024, 1, 1), result.OperationalEndDate);
            Assert.True(result.OfferedInEngland);
            Assert.False(result.FundedInEngland);
            Assert.Equal("100", result.Glh);
            Assert.Equal("200", result.Tqt);
            Assert.True(result.PreSixteen);
            Assert.False(result.SixteenToEighteen);
            Assert.True(result.EighteenPlus);
            Assert.False(result.NineteenPlus);
            Assert.Equal("FS1", result.FundingStreamName);
            Assert.Equal(new DateTime(2023, 9, 1), result.FundingApprovalStartDate);
            Assert.Equal("Extend", result.ProposedOutcome);
            Assert.Equal("Extend", result.RollOverStatus);
            Assert.Equal("None", result.ExclusionReason);
            Assert.Equal(new DateTime(2024, 7, 31), result.CurrentFundingApprovalEndDate);
            Assert.Equal(new DateTime(2025, 7, 31), result.ProposedFundingApprovalEndDate);
            Assert.Equal("Some comments", result.Comments);
        }
    }
}
