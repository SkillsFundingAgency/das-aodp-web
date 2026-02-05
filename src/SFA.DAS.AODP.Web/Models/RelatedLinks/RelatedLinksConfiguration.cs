using SFA.DAS.AODP.Models.Users;

namespace SFA.DAS.AODP.Web.Models.RelatedLinks
{
    public static class RelatedLinksConfiguration
    {
        public static readonly RelatedLink FundingApprovalManual =
            new(
                "Qualification funding approval - Guidance - GOV.UK",
                "https://www.gov.uk/guidance/qualification-funding-approval",
                targetName: "funding_approval"
            );

        public static readonly RelatedLink OfqualRegulation =
            new(
                "Ofqual - GOV.UK",
                "https://www.gov.uk/government/organisations/ofqual",
                targetName: "ofqual"
            );

        public static readonly RelatedLink SkillsEnglandOccupationalMaps =
            new(
                "Occupational Maps: Skills England",
                "https://occupational-maps.skillsengland.education.gov.uk/",
                targetName: "skills_map"
            );

        public static readonly RelatedLink FundedQualifications =
            new(
                "List of Qualifications approved for funding",
                "https://www.qualifications.education.gov.uk/Home/FurtherInformation",
                targetName: "funded_qualifications"
            );

        public static IReadOnlyList<RelatedLink> ForUser(UserType userType)
            => userType switch
            {
                UserType.AwardingOrganisation =>
                [
                    FundingApprovalManual,
                    OfqualRegulation,
                    SkillsEnglandOccupationalMaps
                ],

                UserType.Qfau =>
                [
                    FundedQualifications
                ],

                UserType.Ofqual =>
                [
                    FundedQualifications
                ],

                UserType.SkillsEngland =>
                [
                    FundedQualifications
                ],

                _ => []
            };
    }
}
