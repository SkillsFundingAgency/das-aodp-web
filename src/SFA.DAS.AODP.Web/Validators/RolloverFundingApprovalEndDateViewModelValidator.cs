using FluentValidation;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

public class RolloverFundingApprovalEndDateViewModelValidator : AbstractValidator<RolloverFundingApprovalEndDateViewModel>
{
    public RolloverFundingApprovalEndDateViewModelValidator()
    {
        RuleFor(x => x.MaxApprovalEndDate)
            .NotNull()
            .WithMessage("Max approval end date is required");

        RuleFor(x => x).Custom((m, ctx) =>
        {
            if (m.MaxApprovalEndDate != null)
                ValidateDateGroup(m.MaxApprovalEndDate, "MaxApprovalEndDate", "Max approval end date", ctx);
        });
    }

    private static void ValidateDateGroup(RolloverFundingApprovalEndDate dateModel, string fieldPrefix, string fieldName, ValidationContext<RolloverFundingApprovalEndDateViewModel> ctx)
    {
        var missing = new List<string>();
        if (!dateModel.Day.HasValue) { missing.Add("day"); ctx.AddFailure($"{fieldPrefix}.Day", "Enter a day"); }
        if (!dateModel.Month.HasValue) { missing.Add("month"); ctx.AddFailure($"{fieldPrefix}.Month", "Enter a month"); }
        if (!dateModel.Year.HasValue) { missing.Add("year"); ctx.AddFailure($"{fieldPrefix}.Year", "Enter a year"); }

        // All missing
        if (missing.Count == 3)
        {
            ctx.AddFailure(fieldPrefix, $"Enter {fieldName}");
            return;
        }

        // Partial missing
        if (missing.Count > 0)
        {
            ctx.AddFailure(fieldPrefix, MissingMessage(missing, fieldName));
            return;
        }

        // Validate day range
        if (dateModel.Day.HasValue && (dateModel.Day < 1 || dateModel.Day > 31))
        {
            ctx.AddFailure($"{fieldPrefix}.Day", "Day must be between 1 and 31");
            ctx.AddFailure(fieldPrefix, "Enter a valid day");
            return;
        }

        // Validate month range
        if (dateModel.Month.HasValue && (dateModel.Month < 1 || dateModel.Month > 12))
        {
            ctx.AddFailure($"{fieldPrefix}.Month", "Month must be between 1 and 12");
            ctx.AddFailure(fieldPrefix, "Enter a valid month");
            return;
        }

        // Validate year is 4 digits
        if (dateModel.Year.HasValue && (dateModel.Year < 1000 || dateModel.Year > 9999))
        {
            ctx.AddFailure($"{fieldPrefix}.Year", "Year must be a 4-digit number");
            ctx.AddFailure(fieldPrefix, "Year must be a 4-digit number");
            return;
        }

        // Parse and validate date
        var date = dateModel.ToDateTime();
        if (date == null)
        {
            ctx.AddFailure(fieldPrefix, $"{fieldName} must be a real date");
            ctx.AddFailure($"{fieldPrefix}.Day", "Enter a valid day");
            ctx.AddFailure($"{fieldPrefix}.Month", "Enter a valid month");
            ctx.AddFailure($"{fieldPrefix}.Year", "Enter a valid year");
            return;
        }

        // Validate date is in the future
        if (date.Value.Date <= DateTime.UtcNow.Date)
        {
            ctx.AddFailure(fieldPrefix, $"{fieldName} must be in the future");
            ctx.AddFailure($"{fieldPrefix}.Day", "Enter a valid day");
            ctx.AddFailure($"{fieldPrefix}.Month", "Enter a valid month");
            ctx.AddFailure($"{fieldPrefix}.Year", "Enter a valid year");
        }
    }

    private static string MissingMessage(IReadOnlyList<string> parts, string fieldName) => parts.Count switch
    {
        1 => $"{fieldName} must include a {parts[0]}",
        2 => $"{fieldName} must include a {parts[0]} and {parts[1]}",
        _ => $"{fieldName} must include a day, month and year"
    };
}
