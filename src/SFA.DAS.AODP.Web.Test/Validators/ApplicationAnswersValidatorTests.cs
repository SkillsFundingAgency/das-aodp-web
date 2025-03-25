using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Models.Forms;
using SFA.DAS.AODP.Web.Models.Application;
using SFA.DAS.AODP.Web.Validators;

namespace SFA.DAS.AODP.Web.Test.Validators;

public class ApplicationAnswersValidatorTest
{
    private readonly ApplicationAnswersValidator _sut;
    private readonly List<IAnswerValidator> _validators = new List<IAnswerValidator>();

    public ApplicationAnswersValidatorTest() => _sut = new(_validators);

    [Fact]
    public void ValidateApplicationPageAnswers_InvalidQuestionType_ExceptionRecorded()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        ModelStateDictionary dictionary = new();
        GetApplicationPageByIdQueryResponse page = new()
        {
            Questions = new()
            {
                new()
                {
                    Id = questionId,
                    Type = "Test"
                }
            }
        };

        ApplicationPageViewModel model = new()
        {
            Questions = new()
            {
                new()
                {
                    Id = questionId,
                    Type = SFA.DAS.AODP.Models.Forms.QuestionType.Date,
                }
            }
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _sut.ValidateApplicationPageAnswers(dictionary, page, model));
    }

    [Fact]
    public void ValidateApplicationPageAnswers_NoValidatorFound_ExceptionRecorded()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        ModelStateDictionary dictionary = new();
        GetApplicationPageByIdQueryResponse page = new()
        {
            Questions = new()
            {
                new()
                {
                    Id = questionId,
                    Type = "Date"
                }
            }
        };

        ApplicationPageViewModel model = new()
        {
            Questions = new()
            {
                new()
                {
                    Id = questionId,
                    Type = SFA.DAS.AODP.Models.Forms.QuestionType.Date,
                }
            }
        };

        // Act & Assert
        Assert.Throws<NotImplementedException>(() => _sut.ValidateApplicationPageAnswers(dictionary, page, model));
    }

    [Fact]
    public void ValidateApplicationPageAnswers_ValidatorFound_ErrorRecorded()
    {
        // Arrange
        Mock<IAnswerValidator> validator = new();
        var questionId = Guid.NewGuid();
        ModelStateDictionary dictionary = new();

        GetApplicationPageByIdQueryResponse page = new()
        {
            Questions = new()
            {
                new()
                {
                    Id = questionId,
                    Type = "Date"
                }
            }
        };

        ApplicationPageViewModel model = new()
        {
            Questions = new()
            {
                new()
                {
                    Id = questionId,
                    Type = SFA.DAS.AODP.Models.Forms.QuestionType.Date,
                    Answer = new()
                    {
                        DateValue = DateOnly.MinValue
                    }
                }
            }
        };

        validator.SetupGet(v => v.QuestionTypes).Returns([QuestionType.Date]);
        validator.Setup(v => v.Validate(page.Questions.First(), model.Questions.First().Answer, model)).Throws(new QuestionValidationFailedException(questionId, "something", "something"));
        _validators.Add(validator.Object);
        // Act
        _sut.ValidateApplicationPageAnswers(dictionary, page, model);

        // Assert
        Assert.False(dictionary.IsValid);
        Assert.Equal(1, dictionary.ErrorCount);
        Assert.True(dictionary.ContainsKey(questionId.ToString()));
        Assert.Equal("something", dictionary[questionId.ToString()].Errors.First().ErrorMessage);
    }

}