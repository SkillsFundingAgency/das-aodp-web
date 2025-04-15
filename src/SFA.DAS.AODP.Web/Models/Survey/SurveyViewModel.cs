using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Survey
{
    public enum SatisfactionScoreIndex
    {
        VeryDissatisfied = 1,
        Dissatisfied = 2,
        NeitherSatisfiedOrDissatisfied = 3,
        Satisfied = 4,
        VerySatisfied = 5
    }

    public class SurveyViewModel
    {
        public string Page { get; set; }

        [DisplayName("Satisfaction score")]
        [Required(ErrorMessage = "Please select a satisfaction score.")]
        public SatisfactionScoreIndex? SatisfactionScore { get; set; }

        [Required(ErrorMessage = "Please provide your comments.")]
        [StringLength(1200, MinimumLength = 1, ErrorMessage = "Comments must not exceed 1200 characters.")]
        public string Comments { get; set; }
    }
}