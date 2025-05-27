using EcommerceWeb.Services.Contarcts;
using EcommerceWeb.Utilities.ApiResult;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EcommerceWeb.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ApiResult<T>> GetAsync<T>(string endpoint)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
        return await HandleResponse<T>(response);
    }

    public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest payload)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, payload);
        return await HandleResponse<TResponse>(response);
    }

    private async Task<ApiResult<T>> HandleResponse<T>(HttpResponseMessage response)
    {
        try
        {
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<T>();
                if (data == null)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Unknown Content Return from Api: " + content + " status code: " + (int)response.StatusCode);
                    return ApiResult.Failure<T>("Unknown Error", "Something went wrong");
                }
                return ApiResult.Success(data, response.StatusCode);
            }

            var contentType = response.Content.Headers.ContentType?.MediaType;

            ValidationProblemDetails? validationProblem = null;
            ProblemDetails? genericProblem = null;

            if (contentType == "application/problem+json" || contentType == "application/json")
            {
                var Content = await response.Content.ReadAsStringAsync();

                validationProblem = JsonConvert.DeserializeObject<ValidationProblemDetails>(Content);

                if (validationProblem != null && validationProblem.Errors.Count > 0)
                {
                    var errorMessages = validationProblem.Errors.SelectMany(e => e.Value).ToList();
                    return ApiResult.Failure<T>("Validation Error", string.Join(", ", errorMessages));
                }

                genericProblem = JsonConvert.DeserializeObject<ProblemDetails>(Content);

                if (genericProblem != null)
                {
                    if (genericProblem.Title == null && genericProblem.Detail == null)
                    {
                        _logger.LogError("Unknown Content Return from Api: " + Content + " status code: " + (int)response.StatusCode);
                    }

                    return ApiResult.Failure<T>(genericProblem.Title ?? "Failed", genericProblem.Detail ?? "Failed");
                }

                return ApiResult.Failure<T>("Unknown Error", "Something went wrong");

            }
            else
            {
                var Content = await response.Content.ReadAsStringAsync();
                _logger.LogError("Unknown Content Return from Api: " + Content + " status code: " + (int)response.StatusCode);
                return ApiResult.Failure<T>("Unknown Error", "Something went wrong");

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return ApiResult.Failure<T>("Unknown Error", "Something went wrong");

        }
    }

}
