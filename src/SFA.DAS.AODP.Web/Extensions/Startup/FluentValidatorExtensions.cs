using FluentValidation;
using SFA.DAS.AODP.Web.Validators;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

[ExcludeFromCodeCoverage]
public static class FluentValidatorExtensions
{
    public static IServiceCollection AddFluentValidators(this IServiceCollection services)
    {
        services.AddTransient<IValidator<RolloverEligibilityDatesViewModel>, RolloverEligibilityDatesViewModelValidator>();
        services.AddTransient<IValidator<RolloverFundingApprovalEndDateViewModel>, RolloverFundingApprovalEndDateViewModelValidator>();

        return services;
    }
}