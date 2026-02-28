using System;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filter;

public class ValidationFilter<T>(IValidator<T> validator) : IAsyncActionFilter where T : class
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var argument = context.ActionArguments.Values.FirstOrDefault(x => x is T) as T;

        if (argument == null)
        {
            await next();
            return;
        }

        var validationResult = await validator.ValidateAsync(argument);
        
        if(!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(x => new { x.PropertyName, x.ErrorMessage });
            
            context.Result = new BadRequestObjectResult(new
            {
                Message = "Validation failed",
                Errors = errors
            });

            return;
        }

        await next();
    }
}
