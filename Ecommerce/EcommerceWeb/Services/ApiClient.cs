using EcommerceWeb.Patterns.Results;
using EcommerceWeb.Services.Contarcts;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

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

    public async Task<Result<T>> GetAsync<T>(string endpoint)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
        return await HandleResponse<T>(response);
    }

    public async Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest payload)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, payload);
        return await HandleResponse<TResponse>(response);
    }

    private async Task<Result<T>> HandleResponse<T>(HttpResponseMessage response)
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
                    return Result.Failure<T>(new Error("Unknown Error","Something went wrong"), (int)response.StatusCode);
                }
                return Result.Success(data);
            }

            var contentType = response.Content.Headers.ContentType?.MediaType;

            ValidationProblemDetails? validationProblem = null;
            ProblemDetails? genericProblem = null;

            if (contentType == "application/problem+json" || contentType == "application/json")
            {
                try
                {
                    validationProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                }
                catch
                {
                    genericProblem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                }
            }

            var errorMessages = new List<string>();

            if (validationProblem?.Errors is not null)
            {
                foreach (var kvp in validationProblem.Errors)
                {
                    foreach (var message in kvp.Value)
                    {
                        errorMessages.Add($"{kvp.Key}: {message}");
                    }
                }
            }
            var error = new Error(
                    title: validationProblem?.Title ?? genericProblem?.Title ?? "Unknown error",
                    message: string.Join("; ", errorMessages)
                );

            return Result.Failure<T>(error, (int)response.StatusCode);
        }catch(Exception ex)
        {
            _logger.LogError(ex.ToString());

            var error = new Error("Unknown Error", "Something went wrong");

            return Result.Failure<T>(error, (int)response.StatusCode);

        }
    }

}
