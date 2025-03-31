using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.ExitSurvey
{
    public enum SatisfactionScoreIndex
    {
        VeryDissatisfied = 1,
        Dissatisfied = 2,
        NeitherSatisfiedOrDissatisfied = 3,
        Satisfied = 4,
        VerySatisfied = 5
    }

    public class ExitSurveyFeedbackViewModel
    {
        public string Page { get; set; }

        [DisplayName("Satisfaction score")]
        [Required]
        public SatisfactionScoreIndex? SatisfactionScore { get; set; }

        [Required]
        public string Comments { get; set; }
    }
}