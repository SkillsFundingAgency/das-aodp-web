//using SFA.DAS.AODP.Models.Exceptions.FormValidation;
//using SFA.DAS.AODP.Models.Forms;
//using SFA.DAS.AODP.Web.Models.Application;

//namespace SFA.DAS.AODP.Web.Validators
//{
//    public class CheckboxValidatorTests : IAnswerValidator
//    {
//        public List<QuestionType> QuestionTypes => [QuestionType.MultiChoice];

//        public void Validate(GetApplicationPageByIdQueryResponse.Question question, ApplicationPageViewModel.Answer? answer, ApplicationPageViewModel model)
//        {
//            var required = question.Required;

//            var min = question.Checkbox.MinNumberOfOptions;
//            var max = question.Checkbox.MaxNumberOfOptions;

//            if (required && (answer == null || answer.MultipleChoiceValues == null))
//            {
//                if (min == 0) min = 1;
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"Please select at least {min} option{(min == 1 ? "" : "s")}.");
//            }

//            if (min is not null && min != 0 && (min > answer!.MultipleChoiceValues?.Count))
//            {
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"Please select at least {min} options.");
//            }

//            if (max is not null && max != 0 && (max < answer!.MultipleChoiceValues?.Count))
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"Please only select up to {max} options.");
//        }
//    }
//}

using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Web.Models.Application;
using SFA.DAS.AODP.Web.Validators;
using System.Collections.Generic;

public class CheckboxValidatorTests
{
    private readonly CheckboxValidator _sut = new();

    [Fact]
    public void Validate_Required_NoAnswerProvided_ExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
        };

        ApplicationPageViewModel.Answer answer =
        new()
        {
            DateValue = null
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.StartsWith("Please select at least", ex.Message);
    }

    [Fact]
    public void Validate_Required_NotEnoughOptionsSelected_ExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
            Checkbox = new()
            {
                MinNumberOfOptions = 2
            }
        };

        ApplicationPageViewModel.Answer answer =
        new()
        {
            MultipleChoiceValues = ["a"]
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.StartsWith("Please select at least", ex.Message);
    }

    [Fact]
    public void Validate_Required_TooManyOptionsSelected_ExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
            Checkbox = new()
            {
                MaxNumberOfOptions = 2
            }
        };

        ApplicationPageViewModel.Answer answer =
        new()
        {
            MultipleChoiceValues = ["a", "b", "c"]
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.StartsWith("Please only select up to", ex.Message);
    }
}
