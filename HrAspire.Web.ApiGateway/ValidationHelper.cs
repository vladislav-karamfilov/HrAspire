﻿namespace HrAspire.Web.ApiGateway;

using System;
using System.Net;
using System.Reflection;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

public static class ValidationHelper
{
    public static EndpointFilterDelegate ValidationFilterFactory(EndpointFilterFactoryContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var validatorDescriptors = GetValidators(context.MethodInfo, context.ApplicationServices);

        return invocationContext => ValidateAsync(validatorDescriptors, invocationContext, next);
    }

    private static async ValueTask<object?> ValidateAsync(
        IEnumerable<ValidatorDescriptor> validationDescriptors,
        EndpointFilterInvocationContext invocationContext,
        EndpointFilterDelegate next)
    {
        foreach (var descriptor in validationDescriptors)
        {
            var argument = invocationContext.Arguments[descriptor.ArgumentIndex];
            if (argument is not null)
            {
                var validationResult = await descriptor.Validator.ValidateAsync(new ValidationContext<object>(argument));
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary(), statusCode: (int)HttpStatusCode.BadRequest);
                }
            }
        }

        return await next.Invoke(invocationContext);
    }

    private static IEnumerable<ValidatorDescriptor> GetValidators(MethodInfo methodInfo, IServiceProvider serviceProvider)
    {
        var parameters = methodInfo.GetParameters();
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            // If route or query string params need to be validated too this should be changed
            if (parameter.GetCustomAttribute<FromBodyAttribute>() is not null)
            {
                var validatorType = typeof(IValidator<>).MakeGenericType(parameter.ParameterType);

                if (serviceProvider.GetService(validatorType) is IValidator validator)
                {
                    yield return new ValidatorDescriptor(i, validator);
                }
            }
        }
    }

    private sealed record ValidatorDescriptor(int ArgumentIndex, IValidator Validator);
}
