using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFA.DAS.AODP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovedQualifications",
                columns: table => new
                {
                    QualificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateOfOfqualDataSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QualificationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AwardingOrganisation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QualificationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QualificationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subcategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SectorSubjectArea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Age1416_FundingAvailable = table.Column<bool>(type: "bit", nullable: false),
                    Age1416_FundingApprovalStartDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Age1416_FundingApprovalEndDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Age1416_Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Age1619_FundingAvailable = table.Column<bool>(type: "bit", nullable: false),
                    Age1619_FundingApprovalStartDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Age1619_FundingApprovalEndDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Age1619_Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocalFlexibilities_FundingAvailable = table.Column<bool>(type: "bit", nullable: false),
                    LocalFlexibilities_FundingApprovalStartDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocalFlexibilities_FundingApprovalEndDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocalFlexibilities_Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LegalEntitlementL2L3_FundingAvailable = table.Column<bool>(type: "bit", nullable: false),
                    LegalEntitlementL2L3_FundingApprovalStartDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LegalEntitlementL2L3_FundingApprovalEndDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LegalEntitlementL2L3_Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LegalEntitlementEnglishandMaths_FundingAvailable = table.Column<bool>(type: "bit", nullable: false),
                    LegalEntitlementEnglishandMaths_FundingApprovalStartDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LegalEntitlementEnglishandMaths_FundingApprovalEndDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LegalEntitlementEnglishandMaths_Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DigitalEntitlement_FundingAvailable = table.Column<bool>(type: "bit", nullable: false),
                    DigitalEntitlement_FundingApprovalStartDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DigitalEntitlement_FundingApprovalEndDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DigitalEntitlement_Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ESFLevel34_FundingAvailable = table.Column<bool>(type: "bit", nullable: false),
                    ESFLevel34_FundingApprovalStartDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ESFLevel34_FundingApprovalEndDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ESFLevel34_Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdvancedLearnerLoans_FundingAvailable = table.Column<bool>(type: "bit", nullable: false),
                    AdvancedLearnerLoans_FundingApprovalStartDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdvancedLearnerLoans_FundingApprovalEndDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdvancedLearnerLoans_Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AwardingOrganisationURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    L3FreeCoursesForJobs_FundingAvailable = table.Column<bool>(type: "bit", nullable: false),
                    L3FreeCoursesForJobs_FundingApprovalStartDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    L3FreeCoursesForJobs_FundingApprovalEndDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    L3FreeCoursesForJobs_Notes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovedQualifications", x => x.QualificationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovedQualifications");
        }
    }
}
