using SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;
using SFA.DAS.AODP.Models.Forms.FormSchema;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Question
{
    public class EditQuestionViewModel
    {
        public Guid Id { get; set; }
        public Guid PageId { get; set; }
        public Guid SectionId { get; set; }
        public Guid FormVersionId { get; set; }

        public int Index { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public QuestionType Type { get; set; }
        [Required]
        public bool Required { get; set; }

        public string? Hint { get; set; } = string.Empty;


        public TextInputOptions TextInput { get; set; } = new();
        public RadioOptions RadioButton { get; set; } = new();

        public class TextInputOptions
        {
            public int? MinLength { get; set; }
            public int? MaxLength { get; set; }

        }

        public class RadioOptions
        {
            public List<RadioOptionItem> MultiChoice { get; set; } = new();
            public AdditionalActions AdditionalFormActions { get; set; } = new();

            public class AdditionalActions
            {
                public Guid? RemoveOption { get; set; }
                public bool AddOption { get; set; }

            }

            public class RadioOptionItem
            {
                public Guid Id { get; set; } = Guid.NewGuid();
                public string Value { get; set; }
            }
        }

        public static EditQuestionViewModel Map(GetQuestionByIdQueryResponse response, Guid formVersionId, Guid sectionId)
        {
            Enum.TryParse(response.Data.Type, out QuestionType type);
            return new()
            {
                PageId = response.Data.PageId,
                Id = response.Data.Id,
                FormVersionId = formVersionId,
                Index = response.Data.Order,
                Hint = response.Data.Hint,
                Required = response.Data.Required,
                Type = type,
                Title = response.Data.Title
            };
        }


    }
}