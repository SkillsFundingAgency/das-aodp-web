using SFA.DAS.AODP.Web.Enums;

namespace SFA.DAS.AODP.Web.Models.Qualifications
{
    public class ChangedQualificationViewModel
    {
        public string QualificationReference { get; set; } = string.Empty;
        public string AwardingOrganisation { get; set; } = string.Empty;
        public string QualificationTitle { get; set; } = string.Empty;
        public string QualificationType { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string AgeGroup { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string SectorSubjectArea { get; set; } = string.Empty;
        public string ChangedFieldNames { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority
        {
            get
            {
                return MapChangedFieldsToPriority();
            }
        }

        private string MapChangedFieldsToPriority()
        {
            var priority = "Green";
            if (!string.IsNullOrWhiteSpace(priority) && Status != ActionTypeEnum.NoActionRequired)
            {
                var changedFields = ChangedFieldNames.Split(',').Select(s => s.Trim()).ToList();
                var redChanges = new List<string>() { "Level", "SSA", "GLH" };
                var yellowChanges = new List<string>()
                {
                    "OrganisationName",
                    "Title",
                    "Type",
                    "TotalCredits",
                    "GradingType",
                    "OfferedInEngland",
                    "PreSixteen",
                    "SixteenToEighteen",
                    "EighteenPlus",
                    "NineteenPlus",
                    "MinimumGLH",
                    "TQT",
                    "OperationalEndDate",
                    "LastUpdatedDate",
                    "Version",
                    "OfferedInternationally"
                };
                if (changedFields.Intersect(redChanges).Any())
                {
                    priority = "Red";
                }
                else if (changedFields.Intersect(yellowChanges).Any())
                {
                    priority = "Yellow";
                }

            }
            return priority;
        }
    }
}
