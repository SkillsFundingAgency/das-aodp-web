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

        public bool Required { get; set; }

        public bool AddAnotherRadioOption { get; set; }

        public Dictionary<string, RoutingPoint> RoutingPoints { get; set; } = new Dictionary<string, RoutingPoint>();
        public TextInputOptions TextInput { get; set; } = new();
        public RadioOptions RadioButton { get; set; } = new();


        public class TextInputOptions
        {
            public int? MinLength { get; set; }
            public int? MaxLength { get; set; }

        }

        public class RadioOptions
        {
            public Dictionary<Guid, string> MultiChoice { get; set; } = new();
            public Guid Remove { get; set; }
            public AdditionalActions AdditionalFormActions { get; set; } = new();

            public class AdditionalActions
            {
                public Guid? RemoveOption { get; set; }
                public bool AddOption { get; set; }

            }

        }


    }
}
