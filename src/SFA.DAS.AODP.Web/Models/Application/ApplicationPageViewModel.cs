using SFA.DAS.AODP.Models.Forms.FormSchema;

namespace SFA.DAS.AODP.Web.Models.Application
{
    public class ApplicationPageViewModel
    {
        public Guid ApplicationId { get; set; }
        public Guid OrganisationId { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }
        public Guid PageId { get; set; }
        public bool IsSubmitted { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }


        public List<Question> Questions { get; set; }

        public class Question
        {
            public Guid Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public QuestionType Type { get; set; }
            public bool Required { get; set; }
            public string? Hint { get; set; } = string.Empty;
            public int Order { get; set; }
            public Answer? Answer { get; set; }

            public TextInputOptions TextInput { get; set; } = new();
            public RadioOptions RadioButton { get; set; } = new();

        }

        public class Answer
        {
            public string? TextValue { get; set; }
            public int? IntegerValue { get; set; }
            public DateTime? DateValue { get; set; }
            public List<string>? MultipleChoiceValue { get; set; }
            public string RadioChoiceValue { get; set; }
        }


        public class TextInputOptions
        {
            public int? MinLength { get; set; }
            public int? MaxLength { get; set; }

        }

        public class RadioOptions
        {
            public List<RadioOptionItem> MultiChoice { get; set; } = new();

            public class RadioOptionItem
            {
                public Guid Id { get; set; }
                public string Value { get; set; }
                public int Order { get; set; }
            }
        }

        public static ApplicationPageViewModel MapToViewModel
        (
            GetApplicationPageByIdQueryResponse value,
            Guid applicationId,
            Guid formVersionId,
            Guid sectionId,
            Guid organisationId
        )
        {
            ApplicationPageViewModel model = new ApplicationPageViewModel()
            {
                ApplicationId = applicationId,
                FormVersionId = formVersionId,
                PageId = value.Id,
                SectionId = sectionId,
                Description = value.Description,
                OrganisationId = organisationId,
                Title = value.Title,
                Order = value.Order,
                Questions = new()
            };

            foreach (var question in value.Questions ?? [])
            {
                Enum.TryParse(question.Type, out QuestionType type);
                Question questionModel = new()
                {
                    Id = question.Id,
                    Order = question.Order,
                    Hint = question.Hint,
                    Required = question.Required,
                    Type = type,
                    Title = question.Title
                };

                if (type == QuestionType.Text)
                {
                    questionModel.TextInput = new()
                    {
                        MinLength = question.TextInput.MinLength,
                        MaxLength = question.TextInput.MaxLength,
                    };
                }
                else if (type == QuestionType.Radio)
                {
                    question.RadioButton.MultiChoice = question.RadioButton.MultiChoice.OrderBy(o => o.Order).ToList();
                    questionModel.RadioButton.MultiChoice = new();
                    foreach (var option in question.RadioButton.MultiChoice ?? [])
                    {
                        questionModel.RadioButton.MultiChoice.Add(new()
                        {
                            Id = option.Id,
                            Value = option.Value,
                            Order = option.Order
                        });
                    }
                }

                model.Questions.Add(questionModel);
            }

            model.Questions = model.Questions.OrderBy(o => o.Order).ToList();
            // TODO link to question answer

            return model;
        }

        public static UpdatePageAnswersCommand MapToCommand(ApplicationPageViewModel model)
        {
            UpdatePageAnswersCommand command = new()
            {
                FormVersionId = model.FormVersionId,
                PageId = model.PageId,
                SectionId = model.SectionId,
                ApplicationId = model.ApplicationId,

            };

            foreach (var question in model.Questions)
            {
                var commandQuestion = new UpdatePageAnswersCommand.Question()
                {
                    QuestionId = question.Id,
                    QuestionType = question.Type.ToString(),
                    Answer = new()
                };

                if (question.Type == QuestionType.Text)
                {
                    commandQuestion.Answer.TextValue = question.Answer.TextValue;
                }

                command.Questions.Add(commandQuestion);
            }

            return command;


        }
    }
}