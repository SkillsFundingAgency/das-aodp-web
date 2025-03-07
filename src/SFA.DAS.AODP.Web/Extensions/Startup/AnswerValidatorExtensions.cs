﻿using SFA.DAS.AODP.Web.Validators;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public static class AnswerValidatorExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddTransient<IApplicationAnswersValidator, ApplicationAnswersValidator>();
        services.AddTransient<IAnswerValidator, TextValidator>();
        services.AddTransient<IAnswerValidator, RadioValidator>();
        services.AddTransient<IAnswerValidator, DateValidator>();
        services.AddTransient<IAnswerValidator, CheckboxValidator>();
        services.AddTransient<IAnswerValidator, NumberValidator>();
        services.AddTransient<IAnswerValidator, FileValidator>();

        return services;
    }
}