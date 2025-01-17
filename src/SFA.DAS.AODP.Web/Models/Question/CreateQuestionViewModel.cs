using SFA.DAS.AODP.Models.Forms.FormSchema;
using SFA.DAS.AODP.Models.Forms.Validators;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Question
{
    public class CreateQuestionViewModel
    {
        public Guid PageKey { get; set; }
        public Guid SectionKey { get; set; }
        public Guid FormVersionId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Question Type")]
        public QuestionType? QuestionType { get; set; }
    }
}
