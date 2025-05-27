namespace EcommerceWeb.Utilities.Results;
public class Result<TValue> : Result
{
    private readonly TValue? _value;
    public TValue Value
    {
        get
        {
            if (!IsValid)
            {
                throw new InvalidOperationException($"Can't access {nameof(Value)} of failure Result");
            }
            return _value!;
        }
    }
    public Result(TValue? value) : base()
    {
        _value = value;
    }
    public Result(TValue? value, Error error,  int code) : base(error, code)
    {
        _value = value;
    }
    public Result(TValue? value, List<Error> errors,int code) : base(errors, code)
    {
        _value = value;
    }
    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);
    public static implicit operator Result<TValue>(TValue value) => Success<TValue>(value);
}