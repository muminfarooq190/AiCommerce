namespace EcommerceWeb.Patterns.Results;
public class Error(string title,string message) : IEquatable<Error>
{
    public string Title { get; } = title;
    public string Message { get; } = message;
    public static bool operator ==(Error? a, Error? b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }
    public static bool operator !=(Error? a, Error? b) => !(a == b);
    public virtual bool Equals(Error? other)
    {
        if (other is null)
        {
            return false;
        }

        return Message == other.Message;
    }
    public override bool Equals(object? obj) => obj is Error error && Equals(error);
    public override int GetHashCode() => HashCode.Combine(Message);
    public override string ToString() =>  Message;
    public static implicit operator string(Error error) => error.Message;
}
