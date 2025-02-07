using SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;
using SFA.DAS.AODP.Models.Forms;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.FormBuilder.Question
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
                public bool AddOption { get; set; }
                public int? RemoveOptionIndex { get; set; }

            }

            public class RadioOptionItem
            {
                public Guid Id { get; set; }
                public string Value { get; set; }
                public int Order { get; set; }
            }
        }

        public static EditQuestionViewModel MapToViewModel(GetQuestionByIdQueryResponse response, Guid formVersionId, Guid sectionId)
        {
            Enum.TryParse(response.Type, out QuestionType type);
            EditQuestionViewModel model = new()
            {
                PageId = response.PageId,
                Id = response.Id,
                FormVersionId = formVersionId,
                SectionId = sectionId,
                Index = response.Order,
                Hint = response.Hint,
                Required = response.Required,
                Type = type,
                Title = response.Title
            };

            if (type == QuestionType.Text)
            {
                model.TextInput = new()
                {
                    MinLength = response.TextInput.MinLength,
                    MaxLength = response.TextInput.MaxLength,
                };
            }
            else if (type == QuestionType.Radio)
            {
                response.RadioOptions = response.RadioOptions.OrderBy(o => o.Order).ToList();
                model.RadioButton.MultiChoice = new();
                foreach (var option in response.RadioOptions)
                {
                    model.RadioButton.MultiChoice.Add(new()
                    {
                        Id = option.Id,
                        Value = option.Value,
                        Order = option.Order
                    });
                }
            }
            return model;
        }

        public static UpdateQuestionCommand MapToCommand(EditQuestionViewModel model)
        {
            var command = new UpdateQuestionCommand()
            {
                Hint = model.Hint,
                Title = model.Title,
                Required = model.Required,
                Id = model.Id,
                SectionId = model.SectionId,
                FormVersionId = model.FormVersionId,
                PageId = model.PageId,

            };

            if (model.Type == QuestionType.Text)
            {
                command.TextInput = new()
                {
                    MinLength = model.TextInput.MinLength,
                    MaxLength = model.TextInput.MaxLength,
                };
            }
            else if (model.Type == QuestionType.Radio)
            {
                command.RadioOptions = new();
                foreach (var option in model.RadioButton.MultiChoice)
                {
                    command.RadioOptions.Add(new()
                    {
                        Id = option.Id,
                        Value = option.Value,
                    });
                }
            }

            return command;
        }


    }
}