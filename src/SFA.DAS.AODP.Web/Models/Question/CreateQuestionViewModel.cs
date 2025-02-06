using SFA.DAS.AODP.Models.Forms;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Question
{
    public class CreateQuestionViewModel
    {
        public Guid PageId { get; set; }
        public Guid SectionId { get; set; }
        public Guid FormVersionId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Question Type")]
        public QuestionType? QuestionType { get; set; }

        public bool Required { get; set; }
    }
}
