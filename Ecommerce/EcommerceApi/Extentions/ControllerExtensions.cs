using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EcommerceApi.Extensions;

public static class ControllerExtensions
{
    public static ObjectResult ApplicationProblem(this ControllerBase controller, string title, string detail, int statusCode, string instance, string errorCode, ModelStateDictionary? modelState)
    {
        ValidationProblemDetails problem;

        if (modelState != null)
        {
            problem = new ValidationProblemDetails(modelState)
            {
                Title = title,
                Detail = detail,
                Status = statusCode,
                Instance = instance,
            };
        }
        else
        {
            problem = new ValidationProblemDetails
            {
                Title = title,
                Detail = detail,
                Status = statusCode,
                Instance = instance,
            };
        }

        problem.Extensions["errorCode"] = errorCode;

        return new ObjectResult(problem);

    }
}
