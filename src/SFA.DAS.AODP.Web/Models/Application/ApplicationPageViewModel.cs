using Newtonsoft.Json.Linq;
using SFA.DAS.AODP.Models.Forms;

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


        public List<Question> Questions { get; set; } = new();

        public class Question
        {
            public Guid Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public QuestionType Type { get; set; }
            public bool Required { get; set; }
            public string? Hint { get; set; } = string.Empty;
            public int Order { get; set; }
            public Answer? Answer { get; set; } = new();

            public TextInputOptions TextInput { get; set; } = new();
            public RadioOptions RadioButton { get; set; } = new();

        }

        public class Answer
        {
            public string? TextValue { get; set; }
            public double? NumberValue { get; set; }
            public DateTime? DateValue { get; set; }
            public List<string>? MultipleChoiceValue { get; set; }
            public string? RadioChoiceValue { get; set; }
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
            Guid organisationId,
            GetApplicationPageAnswersByPageIdQueryResponse answers
        )
        {
            ApplicationPageViewModel model = new ApplicationPageViewModel()
            {
                ApplicationId = applicationId,
                FormVersionId = formVersionId,
                PageId = value.Id,
                SectionId = sectionId,
                OrganisationId = organisationId,
            };

            return PopulateViewModel(model, value, answers);
        }

        public static ApplicationPageViewModel RepopulatePageDataOnViewModel
       (
           GetApplicationPageByIdQueryResponse value,
           ApplicationPageViewModel viewModel
       )
        {
            return PopulateViewModel(viewModel, value);

        }

        private static ApplicationPageViewModel PopulateViewModel
        (
            ApplicationPageViewModel model,
            GetApplicationPageByIdQueryResponse value,

            GetApplicationPageAnswersByPageIdQueryResponse? answers = null
        )
        {
            model.Description = value.Description;
            model.Title = value.Title;
            model.Order = value.Order;
            model.Questions ??= new();

            foreach (var question in value.Questions ?? [])
            {
                Enum.TryParse(question.Type, out QuestionType type);
                var questionModel = model.Questions.FirstOrDefault(x => x.Id == question.Id);

                if (questionModel == null)
                {
                    questionModel = new();
                    model.Questions.Add(questionModel);

                }


                questionModel.Id = question.Id;
                questionModel.Order = question.Order;
                questionModel.Hint = question.Hint;
                questionModel.Required = question.Required;
                questionModel.Type = type;
                questionModel.Title = question.Title;


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

            }

            model.Questions = model.Questions.OrderBy(o => o.Order).ToList();
            if (answers != null) PopulateExistingAnswers(model.Questions, answers);

            return model;
        }

        private static void PopulateExistingAnswers(List<Question> questions, GetApplicationPageAnswersByPageIdQueryResponse answers)
        {
            foreach (var question in questions ?? [])
            {
                var answer = answers.Questions.FirstOrDefault(a => a.QuestionId == question.Id)?.Answer;
                if (question.Type == QuestionType.Text)
                {
                    question.Answer.TextValue = answer?.TextValue;

                }
                else if (question.Type == QuestionType.Radio)
                {
                    question.Answer.RadioChoiceValue = answer?.RadioChoiceValue;
                }
            }

        }

        public static UpdatePageAnswersCommand MapToCommand(ApplicationPageViewModel model, GetApplicationPageByIdQueryResponse page)
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
                else if (question.Type == QuestionType.Radio)
                {
                    commandQuestion.Answer.RadioChoiceValue = question.Answer.RadioChoiceValue;

                    var routes = page.Questions.First(p => p.Id == question.Id).Routes;

                    if (routes != null && routes.Any())
                    {
                        var relevantRoute = routes.FirstOrDefault(r => r.OptionId.ToString() == question.Answer.RadioChoiceValue);
                        if (relevantRoute != null)
                        {
                            command.Routing = new()
                            {
                                OptionId = relevantRoute.OptionId,
                                EndForm = relevantRoute.EndForm,
                                EndSection = relevantRoute.EndSection,
                                NextPageOrder = relevantRoute.NextPageOrder,
                                NextSectionOrder = relevantRoute.NextSectionOrder,
                                QuestionId = question.Id,
                            };
                        }
                    }

                }

                command.Questions.Add(commandQuestion);
            }

            return command;


        }
    }
}