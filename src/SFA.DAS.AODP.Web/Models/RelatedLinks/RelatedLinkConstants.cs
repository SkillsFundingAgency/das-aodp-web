namespace SFA.DAS.AODP.Web.Models.RelatedLinks
{
    public static class RelatedLinkConstants
    {
        public static class Text
        {
            public const string ViewApplication = "View application";
            public const string ViewMessages = "View messages";

            public const string FundingApprovalManual = "Qualification funding approval - Guidance - GOV.UK";
            public const string OfqualRegulation = "Ofqual - GOV.UK";
            public const string SkillsEnglandOccupationalMaps = "Occupational Maps: Skills England";
            public const string FundedQualifications = "List of Qualifications approved for funding";
        }

        public static class Urls
        {
            public const string FundingApprovalManual = "https://www.gov.uk/guidance/qualification-funding-approval";
            public const string OfqualRegulation = "https://www.gov.uk/government/organisations/ofqual";
            public const string SkillsEnglandOccupationalMaps = "https://occupational-maps.skillsengland.education.gov.uk/";
            public const string FundedQualifications = "https://www.qualifications.education.gov.uk/Home/FurtherInformation";
        }

        public static class Targets
        {
            public const string FundingApproval = "funding_approval";
            public const string Ofqual = "ofqual";
            public const string SkillsMap = "skills_map";
            public const string FundedQualifications = "funded_qualifications";
        }
    }

}
