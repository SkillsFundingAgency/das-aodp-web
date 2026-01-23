using SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;
using SFA.DAS.AODP.Models.Forms;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Constants;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.FormBuilder.Question
{
    public class EditQuestionViewModel :IValidatableObject
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
        public bool DoesHaveAssociatedRoutes { get; set; } = false;
        public string? Helper { get; set; }
        public string? HelperHTML { get; set; }

        public TextInputOptions TextInput { get; set; } = new();
        public Option Options { get; set; } = new();
        public CheckboxOptions Checkbox { get; set; } = new();
        public NumberInputOptions NumberInput { get; set; } = new();
        public DateInputOptions DateInput { get; set; } = new();
        public FileUploadOptions FileUpload { get; set; } = new();


        public bool Editable { get; set; }

        public AdditionalFormActions? AdditionalActions { get; set; } = new();


        public class AdditionalFormActions
        {
            public bool? UpdateDescriptionPreview { get; set; }
            public bool? SaveAndExit { get; set; }
            public bool? SaveAndAddAnother { get; set; }
        }

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
                public string? Value { get; set; } = string.Empty;
                public int Order { get; set; }
                public bool DoesHaveAssociatedRoutes { get; set; } = false;
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
            [DisplayName("Minimum date")]
            public DateOnly? GreaterThanOrEqualTo { get; set; }
            [DisplayName("Maximum date")]
            public DateOnly? LessThanOrEqualTo { get; set; }
            public RelativeDateValidation DateValidation { get; set; }
            public enum RelativeDateValidation { MustBeInFuture, MustBeInPast, NotApplicable };
            public bool ReadOnly { get; set; } 
        }

        public class FileUploadOptions
        {
            public List<string>? FileTypes { get; set; } = new();
            public string? FileNamePrefix { get; set; }
            public int NumberOfFiles { get; set; }
        }

        public static EditQuestionViewModel MapToViewModel(GetQuestionByIdQueryResponse response, Guid formVersionId, Guid sectionId, FormBuilderSettings settings)
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
                Helper = response.Helper,
                HelperHTML = response.HelperHTML,
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
                        Order = option.Order,
                        DoesHaveAssociatedRoutes = response.Routes.Any(r => r.Option.Id == option.Id &&
                                                                      (r.NextPage?.Id != Guid.Empty || r.NextSection?.Id != Guid.Empty || r.EndForm == true || r.EndSection == true))
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
                    DateValidation = DateInputOptions.RelativeDateValidation.NotApplicable,
                    ReadOnly = !response.Editable,
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
            else if (type == QuestionType.File)
            {
                model.FileUpload = new()
                {
                    FileTypes = settings.UploadFileTypesAllowed,
                    FileNamePrefix = response.FileUpload?.FileNamePrefix,
                    NumberOfFiles = response.FileUpload?.NumberOfFiles ?? settings.MaxUploadNumberOfFiles,
                };
            }
            return model;
        }

        public static UpdateQuestionCommand MapToCommand(EditQuestionViewModel model)
        {
            var command = new UpdateQuestionCommand()
            {
                Hint = model.Hint,
                Helper = model.Helper,
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
            else if (model.Type == QuestionType.File)
            {
                command.FileUpload = new()
                {
                    FileNamePrefix = model.FileUpload.FileNamePrefix,
                    NumberOfFiles = model.FileUpload.NumberOfFiles,
                };
            }

            return command;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var settings = validationContext.GetService(typeof(FormBuilderSettings)) as FormBuilderSettings;
            if (settings == null)
            {
                yield return new ValidationResult("FormBuilderSettings are not configured.");
                yield break;
            }

            if (Type == AODP.Models.Forms.QuestionType.File)
            {
                if (FileUpload.NumberOfFiles > settings.MaxUploadNumberOfFiles)
                {
                    yield return new ValidationResult(
                        string.Format(FormBuilderValidationMessages.TooManyFiles, settings.MaxUploadNumberOfFiles),
                        new[] { $"{nameof(FileUpload)}.{nameof(FileUpload.NumberOfFiles)}" });
                }

                if (Required && FileUpload.NumberOfFiles == 0)
                {
                    yield return new ValidationResult(
                        string.Format(FormBuilderValidationMessages.MandatoryFileUploadMaxFiles, settings.MaxUploadNumberOfFiles),
                        new[] { $"{nameof(FileUpload)}.{nameof(FileUpload.NumberOfFiles)}" });
                }
            }
            else if (Options?.Options != null)
            {
                foreach (var item in Options.Options
                             .Select((option, index) => new { Option = option, Index = index })
                             .Where(item => string.IsNullOrWhiteSpace(item.Option.Value)))
                {
                    yield return new ValidationResult(
                        FormBuilderValidationMessages.OptionTextCannotBeEmpty,
                        new[] { $"{nameof(Options)}.{nameof(Options.Options)}[{item.Index}].Value" });
                }
            }

            if (Type == QuestionType.Text || Type == QuestionType.TextArea)
            {
                var min = TextInput?.MinLength;
                var max = TextInput?.MaxLength;

                if (min.HasValue && min.Value <= 0)
                {
                    yield return new ValidationResult(
                        FormBuilderValidationMessages.MinWordsMustBeGreaterThanZero,
                        new[] { $"{nameof(TextInput)}.{nameof(TextInput.MinLength)}" });
                }

                if (min.HasValue && max.HasValue && max.Value < min.Value)
                {
                    yield return new ValidationResult(
                        FormBuilderValidationMessages.MaxWordsMustBeGreaterThanOrEqualToMin,
                        new[] { $"{nameof(TextInput)}.{nameof(TextInput.MaxLength)}" });
                }

                if (max.HasValue && max.Value <= 0)
                {
                    yield return new ValidationResult(
                        FormBuilderValidationMessages.MaxWordsMustBeGreaterThanZero,
                        new[] { $"{nameof(TextInput)}.{nameof(TextInput.MaxLength)}" });
                }
            }

            if (Type == QuestionType.Number)
            {
                var min = NumberInput?.GreaterThanOrEqualTo;
                var max = NumberInput?.LessThanOrEqualTo;

                if (min.HasValue && max.HasValue && max.Value < min.Value)
                {
                    yield return new ValidationResult(
                        FormBuilderValidationMessages.MaxNumberMustBeGreaterThanOrEqualToMin,
                        new[] { $"{nameof(NumberInput)}.{nameof(NumberInput.LessThanOrEqualTo)}" });
                }
            }

            if (Type == QuestionType.Date)
            {
                var min = DateInput?.GreaterThanOrEqualTo;
                var max = DateInput?.LessThanOrEqualTo;

                if (min.HasValue && max.HasValue && max.Value < min.Value)
                { 
                    yield return new ValidationResult(
                        FormBuilderValidationMessages.MaxDateMustBeLaterThanOrEqualToMin,
                        new[] { $"{nameof(DateInput)}.{nameof(DateInput.LessThanOrEqualTo)}" });
                }
            }

            if (Type == QuestionType.MultiChoice)
            {
                var min = Checkbox.MinNumberOfOptions;
                var max = Checkbox?.MaxNumberOfOptions;

                if (min.HasValue && max.HasValue && max.Value < min.Value)
                {
                    yield return new ValidationResult(
                        FormBuilderValidationMessages.MinOptionsCannotBeGreaterThanMaxOptions,
                        new[] { $"{nameof(Checkbox)}.{nameof(Checkbox.MinNumberOfOptions)}" });
                }

                var numberOfOptions = Options?.Options?.Count ?? 0;
                if (min.HasValue && min.Value > numberOfOptions)
                {
                    yield return new ValidationResult(
                        FormBuilderValidationMessages.MinOptionsCannotBeGreaterThanAvailableOptions,
                        new[] { $"{nameof(Checkbox)}.{nameof(Checkbox.MinNumberOfOptions)}" });
                }
            }
        }
    }
}