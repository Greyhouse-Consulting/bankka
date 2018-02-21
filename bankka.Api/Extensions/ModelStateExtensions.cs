using bankka.Api.Controllers;
using bankka.Api.Models;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace bankka.Api.Extensions
{
    public static class ModelStateExtensions
    {
        public static ErrorModel ToErrorModel(this ModelStateDictionary modelState, string code, string message)
        {
            var model = new ErrorModel(code, message);
            foreach (var state in modelState)
            {
                model.Properties.Add(new ErrorModelProperty
                {
                    Code = state.Key,
                    Field = state.Key,
                });
            }

            return model;
        }

        public static ErrorModel ToErrorModel(this ValidationResult validationResult, string code, string message)
        {
            var model = new ErrorModel(code, message);

            foreach (var error in validationResult.Errors)
            {
                model.Properties.Add(new ErrorModelProperty
                {
                    Code = error.ErrorCode,
                    Field = error.PropertyName,
                });
            }

            return model;
        }
    }
}