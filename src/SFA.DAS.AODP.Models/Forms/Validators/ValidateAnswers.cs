using SFA.DAS.AODP.Models.Forms.Application;
using SFA.DAS.AODP.Models.Forms.FormSchema;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using System.Collections.Concurrent;

namespace SFA.DAS.AODP.Models.Forms.Validators;

public class AnswerValidation {

    public static ConcurrentDictionary<int,string> exceptions { get; set;}
    public static List<AnsweredQuestion> answers {get; set;}

    public static async Task Main() {
        List<AnsweredQuestion> listOfAnswers = new List<AnsweredQuestion> {
            // Should result in pass
            new AnsweredQuestion {
                Id = 1,
                QuestionSchemaId = 1,
                Index = 1,
                AnsweredStatus = AnsweredStatus.Answered,
                Type = QuestionType.Text,
                TextValue = "This will pass!"
            },
            // Should result in pass
            new AnsweredQuestion {
                Id = 2,
                QuestionSchemaId = 1,
                Index = 2,
                AnsweredStatus = AnsweredStatus.Answered,
                Type = QuestionType.Integer,
                IntegerValue = 11
            },
            // Should result in pass
            new AnsweredQuestion {
                Id = 3,
                QuestionSchemaId = 1,
                Index = 3,
                AnsweredStatus = AnsweredStatus.Answered,
                Type = QuestionType.Decimal,
                DecimalValue = 11f
            },
            // Should result in pass
            new AnsweredQuestion {
                Id = 4,
                QuestionSchemaId = 1,
                Index = 4,
                AnsweredStatus = AnsweredStatus.Answered,
                Type = QuestionType.Date,
                DateValue = new DateTime(2025, 1, 21)
            },
            // Should result in pass
            new AnsweredQuestion {
                Id = 5,
                QuestionSchemaId = 1,
                Index = 5,
                AnsweredStatus = AnsweredStatus.Answered,
                Type = QuestionType.MultiChoice,
                MultipleChoiceValue = "This too shall pass"
            },
            // Should result in pass
            new AnsweredQuestion {
                Id = 6,
                QuestionSchemaId = 1,
                Index = 6,
                AnsweredStatus = AnsweredStatus.Answered,
                Type = QuestionType.Boolean,
                BooleanValue = true
            },
            // Should result in exception
            new AnsweredQuestion {
                Id = 7,
                QuestionSchemaId = 1,
                Index = 7,
                AnsweredStatus = AnsweredStatus.Answered,
                Type = QuestionType.Text,
                TextValue = "This will fail"
            },
            // Should result in exception
            new AnsweredQuestion {
                Id = 8,
                QuestionSchemaId = 1,
                Index = 8,
                AnsweredStatus = AnsweredStatus.Answered,
                Type = QuestionType.Integer,
                IntegerValue = 9
            },
            // Should result in exception
            new AnsweredQuestion {
                Id = 9,
                QuestionSchemaId = 1,
                Index = 9,
                AnsweredStatus = AnsweredStatus.Answered,
                Type = QuestionType.Decimal,
                DecimalValue = 10f
            },
            // Should result in exception
            new AnsweredQuestion {
                Id = 10,
                QuestionSchemaId = 1,
                Index = 10,
                AnsweredStatus = AnsweredStatus.Answered,
                Type = QuestionType.Date,
                DateValue = new DateTime(2025, 1, 19)
            },
            // Should result in pass
            new AnsweredQuestion {
                Id = 11,
                QuestionSchemaId = 1,
                Index = 11,
                AnsweredStatus = AnsweredStatus.Answered,
                Type = QuestionType.MultiChoice,
                MultipleChoiceValue = null
            },
            // Should result in pass
            new AnsweredQuestion {
                Id = 12,
                QuestionSchemaId = 1,
                Index = 12,
                AnsweredStatus = AnsweredStatus.Answered,
                Type = QuestionType.Boolean,
                BooleanValue = null
            }
        };
        ConcurrentDictionary<int,string> listOfExceptions = ValidateAnswers(listOfAnswers);
        foreach (var item in listOfExceptions)
        {
            Console.WriteLine(item.Key + ":" + item.Value);
        }
    }
    public static ConcurrentDictionary<int, string> ValidateAnswers(List<AnsweredQuestion> answers)
    {
        var questionSchemaText = new QuestionSchema
        {
            Id = 1,
            PageId = 1,
            SectionId = 1,
            FormId = 1,
            Index = 1,
            Title = "Title",
            Hint = "Hint",
            Type = QuestionType.Text,
            TextValidator = new TextValidator
            {
                Required = true,
                MinLength = 15,
                MaxLength = 22
            }
        };

        var questionSchemaInteger = new QuestionSchema
        {
            Id = 2,
            PageId = 1,
            SectionId = 1,
            FormId = 1,
            Index = 2,
            Title = "Title",
            Hint = "Hint",
            Type = QuestionType.Integer,
            IntegerValidator = new IntegerValidator
            {
                Required = true,
                GreaterThan = 10,
                LessThan = 22
            }
        };

        var questionSchemaDecimal = new QuestionSchema
        {
            Id = 3,
            PageId = 1,
            SectionId = 1,
            FormId = 1,
            Index = 3,
            Title = "Title",
            Hint = "Hint",
            Type = QuestionType.Decimal,
            DecimalValidator = new DecimalValidator
            {
                Required = true,
                GreaterThan = 10.5f,
                LessThan = 22.5f
            }
        };

        var questionSchemaDate = new QuestionSchema
        {
            Id = 4,
            PageId = 1,
            SectionId = 1,
            FormId = 1,
            Index = 4,
            Title = "Title",
            Hint = "Hint",
            Type = QuestionType.Date,
            DateValidator = new DateValidator
            {
                Required = true,
                GreaterThanTimeInPast = new TimeSpan(2025, 1, 20)
            }
        };

        var questionSchemaMultiChoice = new QuestionSchema
        {
            Id = 5,
            PageId = 1,
            SectionId = 1,
            FormId = 1,
            Index = 5,
            Title = "Title",
            Hint = "Hint",
            Type = QuestionType.MultiChoice,
            MultiChoiceValidator = new MultiChoiceValidator
            {
                Required = true,
            }
        };

        var questionSchemaBoolean = new QuestionSchema
        {
            Id = 6,
            PageId = 1,
            SectionId = 1,
            FormId = 1,
            Index = 6,
            Title = "Title",
            Hint = "Hint",
            Type = QuestionType.Boolean,
            BooleanValidator = new BooleanValidator
            {
                Required = true,
            }
        };

        exceptions = new ConcurrentDictionary<int, string>();

        foreach (var answer in answers)
        {
            try
            {
                switch (answer.Type)
                {
                    default:
                        questionSchemaText.Validate(answer);
                        break;
                    case QuestionType.Integer:
                        questionSchemaInteger.Validate(answer);
                        break;
                    case QuestionType.Decimal:
                        questionSchemaDecimal.Validate(answer);
                        break;
                    case QuestionType.Date:
                        questionSchemaDate.Validate(answer);
                        break;
                    case QuestionType.MultiChoice:
                        questionSchemaMultiChoice.Validate(answer);
                        break;
                    case QuestionType.Boolean:
                        questionSchemaBoolean.Validate(answer);
                        break;
                }
            }
            catch (Exception questionValidationError)
            {
                exceptions.GetOrAdd(answer.Id, questionValidationError.Message);
                // string exceptionMessage = questionValidationError.Message;
                // exceptions.Add(answer.Id, exceptionMessage);
            }
        }

        return exceptions;
    }
}
