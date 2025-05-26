using EcommerceWeb.Patterns.Results;

namespace EcommerceWeb.Services.Contarcts;

public interface IApiClient
{
    Task<Result<T>> GetAsync<T>(string endpoint);
    Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest payload);
}
