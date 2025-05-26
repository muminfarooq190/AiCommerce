namespace EcommerceWeb.Patterns.Results;

public class Result
{
    public Result(int code = 200)
    {
        IsValid = true;
        Code = code;
    }
    public Result(Error error, int code)
    {
        
        IsValid = false;
        _error.Add(error);
        Code = code;
    }
    public Result(List<Error> error, int code)
    {
       
        IsValid = false;
        _error.AddRange(error);
        Code = code;
    }
    private readonly List<Error> _error = [];
    public int Code { get; }
    public IReadOnlyCollection<Error> Errors => _error.AsReadOnly();
    public bool IsValid { get; }
    public static Result Success() => new();
    public static Result<TValue> Success<TValue>(TValue value) => new(value);
    public static Result Failure(Error error, int code = 400) => new(error, code);
    public static Result Failure(List<Error> errors, int code = 400) => new(errors, code);
    public static Result<TValue> Failure<TValue>(Error error, int code = 400) => new(default, error, code);
    public static Result<TValue> Failure<TValue>(List<Error> errors,  int code = 400) => new(default, errors, code);
    public static implicit operator Result(Error error) => Failure(error);
}
