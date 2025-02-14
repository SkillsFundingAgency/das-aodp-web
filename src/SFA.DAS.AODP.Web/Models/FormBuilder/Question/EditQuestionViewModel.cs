using Azure;
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
        public Option Options { get; set; } = new();
        public CheckboxOptions Checkbox { get; set; } = new();
        public NumberInputOptions NumberInput { get; set; } = new();
        public DateInputOptions DateInput { get; set; } = new();


        public bool Editable { get; set; }

        public class TextInputOptions
        {
            public int? MinLength { get; set; }
            public int? MaxLength { get; set; }

        }

        public class Option
        {
            public List<OptionItem> Options { get; set; } = new();
            public AdditionalActions AdditionalFormActions { get; set; } = new();

            public class AdditionalActions
            {
                public bool AddOption { get; set; }
                public int? RemoveOptionIndex { get; set; }

            }

            public class OptionItem
            {
                public Guid Id { get; set; }
                public string Value { get; set; } = string.Empty;
                public int Order { get; set; }
            }
        }

        public class CheckboxOptions
        {
            public int? MinNumberOfOptions { get; set; }
            public int? MaxNumberOfOptions { get; set; }
        }

        public class NumberInputOptions
        {
            public int? GreaterThanOrEqualTo { get; set; }
            public int? LessThanOrEqualTo { get; set; }
            public int? NotEqualTo { get; set; }
        }

        public class DateInputOptions
        {
            public DateOnly? GreaterThanOrEqualTo { get; set; }
            public DateOnly? LessThanOrEqualTo { get; set; }
            public RelativeDateValidation DateValidation { get; set; }
            public enum RelativeDateValidation { MustBeInFuture, MustBeInPast, NotApplicable };
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
                Title = response.Title,
                Editable = response.Editable,
            };

            if (type == QuestionType.Text || type == QuestionType.TextArea)
            {
                model.TextInput = new()
                {
                    MinLength = response.TextInput.MinLength,
                    MaxLength = response.TextInput.MaxLength,
                };
            }
            else if (type == QuestionType.Radio || type == QuestionType.MultiChoice)
            {
                response.Options = response.Options.OrderBy(o => o.Order).ToList();
                foreach (var option in response.Options)
                {
                    model.Options.Options.Add(new()
                    {
                        Id = option.Id,
                        Value = option.Value,
                        Order = option.Order
                    });
                }

                if (type == QuestionType.MultiChoice)
                {
                    model.Checkbox = new()
                    {
                        MaxNumberOfOptions = response.Checkbox?.MaxNumberOfOptions ?? 0,
                        MinNumberOfOptions = response.Checkbox?.MinNumberOfOptions ?? 0,
                    };
                }
            }
            else if (type == QuestionType.Number)
            {
                model.NumberInput = new()
                {
                    GreaterThanOrEqualTo = response.NumberInput.GreaterThanOrEqualTo,
                    LessThanOrEqualTo = response.NumberInput.LessThanOrEqualTo,
                    NotEqualTo = response.NumberInput.NotEqualTo,
                };
            }
            else if (type == QuestionType.Date)
            {
                model.DateInput = new()
                {
                    GreaterThanOrEqualTo = response.DateInput.GreaterThanOrEqualTo,
                    LessThanOrEqualTo = response.DateInput.LessThanOrEqualTo,
                    DateValidation = DateInputOptions.RelativeDateValidation.NotApplicable
                };

                if (response.DateInput.MustBeInFuture == true)
                {
                    model.DateInput.DateValidation = DateInputOptions.RelativeDateValidation.MustBeInFuture;
                }
                else if (response.DateInput.MustBeInPast == true)
                {
                    model.DateInput.DateValidation = DateInputOptions.RelativeDateValidation.MustBeInPast;
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

            if (model.Type == QuestionType.Text || model.Type == QuestionType.TextArea)
            {
                command.TextInput = new()
                {
                    MinLength = model.TextInput.MinLength,
                    MaxLength = model.TextInput.MaxLength,
                };
            }
            else if (model.Type == QuestionType.Radio || model.Type == QuestionType.MultiChoice)
            {
                command.Options = new();
                foreach (var option in model.Options.Options.OrderBy(o => o.Order))
                {
                    command.Options.Add(new()
                    {
                        Id = option.Id,
                        Value = option.Value ?? string.Empty,
                    });
                }

                if (model.Type == QuestionType.MultiChoice)
                {
                    command.Checkbox = new()
                    {
                        MinNumberOfOptions = model.Checkbox.MinNumberOfOptions,
                        MaxNumberOfOptions = model.Checkbox.MaxNumberOfOptions,
                    };
                }
            }
            else if (model.Type == QuestionType.Number)
            {
                command.NumberInput = new()
                {
                    GreaterThanOrEqualTo = model.NumberInput.GreaterThanOrEqualTo,
                    LessThanOrEqualTo = model.NumberInput.LessThanOrEqualTo,
                    NotEqualTo = model.NumberInput.NotEqualTo,
                };
            }
            else if (model.Type == QuestionType.Date)
            {
                command.DateInput = new()
                {
                    GreaterThanOrEqualTo = model.DateInput.GreaterThanOrEqualTo,
                    LessThanOrEqualTo = model.DateInput.LessThanOrEqualTo,
                    MustBeInFuture = model.DateInput.DateValidation == DateInputOptions.RelativeDateValidation.MustBeInFuture,
                    MustBeInPast = model.DateInput.DateValidation == DateInputOptions.RelativeDateValidation.MustBeInPast,
                };
            }

            return command;
        }
    }
}