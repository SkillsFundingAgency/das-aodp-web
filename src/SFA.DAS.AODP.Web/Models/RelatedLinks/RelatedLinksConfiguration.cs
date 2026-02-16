using SFA.DAS.AODP.Models.Users;

namespace SFA.DAS.AODP.Web.Models.RelatedLinks
{
    public static class RelatedLinksConfiguration
    {
        public static readonly RelatedLink FundingApprovalManual =
            new(RelatedLinkConstants.Text.FundingApprovalManual, RelatedLinkConstants.Urls.FundingApprovalManual, targetName: RelatedLinkConstants.Targets.FundingApproval);

        public static readonly RelatedLink OfqualRegulation =
            new(RelatedLinkConstants.Text.OfqualRegulation, RelatedLinkConstants.Urls.OfqualRegulation, targetName: RelatedLinkConstants.Targets.Ofqual);

        public static readonly RelatedLink SkillsEnglandOccupationalMaps =
            new(RelatedLinkConstants.Text.SkillsEnglandOccupationalMaps, RelatedLinkConstants.Urls.SkillsEnglandOccupationalMaps, targetName: RelatedLinkConstants.Targets.SkillsMap);

        public static readonly RelatedLink FundedQualifications =
            new(RelatedLinkConstants.Text.FundedQualifications, RelatedLinkConstants.Urls.FundedQualifications, targetName: RelatedLinkConstants.Targets.FundedQualifications);

        public static IReadOnlyList<RelatedLink> ForUser(UserType userType) => userType switch
        {
            UserType.AwardingOrganisation => [FundingApprovalManual, OfqualRegulation, SkillsEnglandOccupationalMaps],
            UserType.Qfau => [FundedQualifications],
            UserType.Ofqual => [FundedQualifications],
            UserType.SkillsEngland => [FundedQualifications],
            _ => []
        };
    }

}
