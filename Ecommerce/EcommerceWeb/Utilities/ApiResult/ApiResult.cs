using EcommerceWeb.Utilities.ApiResult.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EcommerceWeb.Utilities.ApiResult;

public class ApiResult<T> : ApiResult
{
    private T? _data;
    public T Data
    {
        get
        {
            if (ResultType != ResultType.Success)
            {
                throw new InvalidOperationException("Data is not available when ResultType is not Success.");
            }
            return _data ?? throw new ArgumentNullException(nameof(Data));
        }
        private set
        {
            _data = value;
            ResultType = ResultType.Success;
        }
    }
    public static ApiResult<T> Success(T value, HttpStatusCode statusCode)
    {
        return new ApiResult<T> { _data = value, StatusCode = statusCode, ResultType = ResultType.Success };

    }   
    public static implicit operator ApiResult<T>(T value) => new ApiResult<T> { Data = value };

}
public class ApiResult
{
    public ResultType ResultType { get; protected set; }
    public HttpStatusCode? StatusCode { get; protected set; }
    public string? Details { get; protected set; }
    public string? Title { get; protected set; }
    public static ApiResult<T> Success<T>(T value, HttpStatusCode statusCode)
    {
        return ApiResult<T>.Success(value, statusCode);

    }
    public static ApiResult Success(HttpStatusCode statusCode)
    {
        return new ApiResult { ResultType = ResultType.Success,StatusCode = statusCode};

    }
    public IReadOnlyDictionary<string, string[]> Errors { get; protected set; } = new Dictionary<string, string[]>();
    public static ApiResult<T> Failure<T>(string errorTitle, string errorMessage)
    {
        return new ApiResult<T>
        {
            ResultType = ResultType.GenericError,
            StatusCode = HttpStatusCode.BadRequest,
            Details = errorMessage,
            Title = errorTitle
        };
    }
    public static ApiResult<T> ValidationError<T>(ValidationProblemDetails validationProblemDetails)
    {
        return new ApiResult<T>
        {
            ResultType = ResultType.ValidationError,
            StatusCode = HttpStatusCode.BadRequest,
            Errors = validationProblemDetails.Errors.AsReadOnly(),
            Details = validationProblemDetails.Detail,
            Title = validationProblemDetails.Title
        };
    }

}