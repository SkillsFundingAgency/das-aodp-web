using SFA.DAS.AODP.Models.Forms.FormSchema;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Question
{
    public class EditQuestionViewModel
    {
        public Guid Id { get; set; }
        public Guid PageKey { get; set; }
        public Guid SectionKey { get; set; }
        public Guid FormId { get; set; }
        public Guid FormVersionId { get; set; }

        public int Index { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Hint { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public List<string> MultiChoice { get; set; } = new List<string>();
        public Dictionary<string, RoutingPoint> RoutingPoints { get; set; } = new Dictionary<string, RoutingPoint>();
        public TextValidation TextValidationDetails { get; set; } = new();


        public class TextValidation
        {
            public bool Required { get; set; }
            public int? MinLength { get; set; }
            public int? MaxLength { get; set; }
        }
    }
}
