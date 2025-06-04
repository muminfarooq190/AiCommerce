using EcommerceWeb.Services.Contarcts;
using EcommerceWeb.Utilities.ApiResult;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace EcommerceWeb.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiClient(IHttpContextAccessor httpContextAccessor, HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpContextAccessor = httpContextAccessor;
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

	public async Task<ApiResult<TResponse>> PostMultipartAsync<TResponse>(string endpoint, IFormFile file)
	{
		using var formData = new MultipartFormDataContent();
		using var fileStream = file.OpenReadStream();

		var fileContent = new StreamContent(fileStream);
		fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);

		formData.Add(fileContent, "file", file.FileName);

		var response = await _httpClient.PostAsync(endpoint, formData);
		return await HandleResponse<TResponse>(response);
	}

	public async Task<ApiResult<T>> DeleteAsync<T>(string endpoint)
	{
		var response = await _httpClient.DeleteAsync(endpoint);
		return await HandleResponse<T>(response);
	}

	public async Task<ApiResult<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest payload)
	{
		var json = JsonConvert.SerializeObject(payload);
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var response = await _httpClient.PutAsync(endpoint, content);
		return await HandleResponse<TResponse>(response);
	}

	private async Task<ApiResult<T>> HandleResponse<T>(HttpResponseMessage response)
    {
        try
        {
            
            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _httpContextAccessor.HttpContext?.Session?.Clear();
                    _httpContextAccessor.HttpContext?.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
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
                    return ApiResult.ValidationError<T>(validationProblem);
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
