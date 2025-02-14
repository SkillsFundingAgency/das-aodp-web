using SFA.DAS.AODP.Models.Forms;
using System.ComponentModel;

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

            public List<Option> Options { get; set; } = new();

        }

        public class Answer
        {
            public string? TextValue { get; set; }
            public decimal? NumberValue { get; set; }

            [DisplayName("Date")]
            public DateOnly? DateValue { get; set; }
            public List<string>? MultipleChoiceValues { get; set; }
            public string? RadioChoiceValue { get; set; }
        }


        public class TextInputOptions
        {
            public int? MinLength { get; set; }
            public int? MaxLength { get; set; }

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


        public class Option
        {
            public Guid Id { get; set; }
            public string Value { get; set; }
            public int Order { get; set; }
        }

        public class DateInputOptions
        {
            public DateOnly? GreaterThanOrEqualTo { get; set; }
            public DateOnly? LessThanOrEqualTo { get; set; }
            public bool? MustBeInFuture { get; set; }
            public bool? MustBeInPast { get; set; }
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

            foreach (var responseQuestion in value.Questions ?? [])
            {
                Enum.TryParse(responseQuestion.Type, out QuestionType type);
                var questionModel = model.Questions.FirstOrDefault(x => x.Id == responseQuestion.Id);

                if (questionModel == null)
                {
                    questionModel = new();
                    model.Questions.Add(questionModel);

                }

                questionModel.Id = responseQuestion.Id;
                questionModel.Order = responseQuestion.Order;
                questionModel.Hint = responseQuestion.Hint;
                questionModel.Required = responseQuestion.Required;
                questionModel.Type = type;
                questionModel.Title = responseQuestion.Title;


                if (type == QuestionType.Radio || type == QuestionType.MultiChoice)
                {
                    responseQuestion.Options = responseQuestion?.Options?.OrderBy(o => o.Order)?.ToList() ?? [];
                    questionModel.Options = new();

                    foreach (var option in responseQuestion?.Options ?? [])
                    {
                        questionModel.Options.Add(new()
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
                if (answer == null) continue;

                if (question.Type == QuestionType.Text || question.Type == QuestionType.TextArea)
                {
                    question.Answer!.TextValue = answer?.TextValue;

                }
                else if (question.Type == QuestionType.Radio)
                {
                    question.Answer!.RadioChoiceValue = answer?.RadioChoiceValue;
                }
                else if (question.Type == QuestionType.MultiChoice)
                {
                    question.Answer!.MultipleChoiceValues = answer?.MultipleChoiceValue;
                }
                else if (question.Type == QuestionType.Number)
                {
                    question.Answer!.NumberValue = answer?.NumberValue;
                }
                else if (question.Type == QuestionType.Date)
                {
                    question.Answer!.DateValue = answer?.DateValue;
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

                if (question.Type == QuestionType.Text || question.Type == QuestionType.TextArea)
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
                else if (question.Type == QuestionType.MultiChoice)
                {
                    commandQuestion.Answer.MultipleChoiceValue = question.Answer.MultipleChoiceValues;
                }
                else if (question.Type == QuestionType.Number)
                {
                    commandQuestion.Answer.NumberValue = question.Answer.NumberValue;
                }
                else if (question.Type == QuestionType.Date)
                {
                    commandQuestion.Answer.DateValue = question.Answer.DateValue;
                }

                command.Questions.Add(commandQuestion);
            }

            return command;
        }
    }
}