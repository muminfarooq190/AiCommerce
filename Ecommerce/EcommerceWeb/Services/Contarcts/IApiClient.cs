using EcommerceWeb.Utilities.ApiResult;

namespace EcommerceWeb.Services.Contarcts;

public interface IApiClient
{
    Task<ApiResult<T>> GetAsync<T>(string endpoint);
    Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest payload);
    Task<ApiResult<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest payload);
    Task<ApiResult<T>> DeleteAsync<T>(string endpoint);

	Task<ApiResult<TResponse>> PostMultipartAsync<TResponse>(string endpoint, IFormFile file);
}
