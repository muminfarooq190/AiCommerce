using EcommerceWeb.Utilities.ApiResult;

namespace EcommerceWeb.Services.Contarcts;

public interface IApiClient
{
    Task<ApiResult<T>> GetAsync<T>(string endpoint);
    Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest payload);
}
