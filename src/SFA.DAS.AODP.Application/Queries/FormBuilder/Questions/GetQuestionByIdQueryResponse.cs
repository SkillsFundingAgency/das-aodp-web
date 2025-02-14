﻿
namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;

public class GetQuestionByIdQueryResponse
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public string Title { get; set; }
    public Guid Key { get; set; }
    public string Hint { get; set; }
    public int Order { get; set; }
    public bool Required { get; set; }
    public string Type { get; set; }

    public TextInputOptions TextInput { get; set; } = new();
    public NumberInputOptions NumberInput { get; set; } = new();
    public CheckboxOptions Checkbox { get; set; } = new();
    public DateInputOptions DateInput { get; set; } = new();
    public List<Option> Options { get; set; } = new();
    public bool Editable { get; set; }

    public class TextInputOptions
    {
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
    }

    public class Option
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
        public int Order { get; set; }
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
        public bool? MustBeInFuture { get; set; }
        public bool? MustBeInPast { get; set; }
    }
}