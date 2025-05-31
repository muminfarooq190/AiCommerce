using EcommerceWeb.Utilities.ApiResult;
using EcommerceWeb.Utilities.ApiResult.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EcommerceWeb.Extensions;

public static class ModelStateExtensions
{
    public static void AddApiResult(this ModelStateDictionary modelState, ApiResult apiResult)
    {

        if (apiResult.ResultType == ResultType.ValidationError)
        {

            foreach (var kvp in apiResult.Errors)
            {
                var key = kvp.Key;
                foreach (var message in kvp.Value)
                {
                    modelState.AddModelError(key, message);
                }
            }
            return;
        }

        if (apiResult.ResultType == ResultType.GenericError)
        {
            modelState.AddModelError("GenericError", apiResult.Details ?? "An error occurred while processing your request.");
            return;
        }
    }
}
