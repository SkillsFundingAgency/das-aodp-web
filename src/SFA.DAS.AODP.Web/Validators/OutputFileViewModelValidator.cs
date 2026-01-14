using FluentValidation;
using SFA.DAS.AODP.Web.Models.OutputFile;

namespace SFA.DAS.AODP.Web.Validators
{
    using FluentValidation;

    public class OutputFileViewModelValidator : AbstractValidator<OutputFileViewModel>
    {
        public OutputFileViewModelValidator()
        {
            RuleFor(x => x.DateChoice)
                .Must(c => c is PublicationDateMode.Today or PublicationDateMode.Manual)
                .WithMessage("Choose a publication date option.");

            RuleFor(x => x).Custom((m, ctx) =>
            {
                if (m.DateChoice == PublicationDateMode.None)
                    return; 

                if (m.DateChoice == PublicationDateMode.Today)
                    return;

                var missing = new List<string>();
                if (!m.Day.HasValue) { missing.Add("day"); ctx.AddFailure("Day", "Enter a day"); }
                if (!m.Month.HasValue) { missing.Add("month"); ctx.AddFailure("Month", "Enter a month"); }
                if (!m.Year.HasValue) { missing.Add("year"); ctx.AddFailure("Year", "Enter a year"); }

                //All missing
                if (missing.Count == 3)
                {
                    ctx.AddFailure("PublicationDate", "Enter a publication date");
                    return;
                }

                if (missing.Count > 0)
                {
                    ctx.AddFailure("PublicationDate", MissingMessage(missing));
                    return;
                }

                if (m.Year is int y && (y < 1000 || y > 9999))
                {
                    ctx.AddFailure("Year", "Year must include 4 numbers.");
                    ctx.AddFailure("PublicationDate", "Year must include 4 numbers.");
                    return;
                }

                if (!m.ParseDate(out var date))
                {
                    ctx.AddFailure("PublicationDate", "Publication date must be a real date");
                    ctx.AddFailure("Day", "Enter a valid day");
                    ctx.AddFailure("Month", "Enter a valid month");
                    ctx.AddFailure("Year", "Enter a valid year");
                    return;
                }

                if (date.Date < DateTime.UtcNow.Date)
                {
                    ctx.AddFailure("PublicationDate", "Publication date must be today or in the future");
                    ctx.AddFailure("Day", "Enter a valid day");
                    ctx.AddFailure("Month", "Enter a valid month");
                    ctx.AddFailure("Year", "Enter a valid year");
                }
            });
        }

        private static string MissingMessage(IReadOnlyList<string> parts) => parts.Count switch
        {
            1 => $"Publication date must include a {parts[0]}.",
            2 => $"Publication date must include a {parts[0]} and {parts[1]}.",
            _ => "Publication date must include a day, month and year."
        };
    }

}
