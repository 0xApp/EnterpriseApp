using System;

namespace EnterpriseApp.Errors;

public readonly struct Error : IEquatable<Error>
{
    public string Code { get; }
    public string Message { get; }

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public Error(string code) : this(code, string.Empty)
    {
    }

    public static implicit operator Error(string code) => new(code);

    public bool Equals(Error other) => Code == other.Code && Message == other.Message;
    public override bool Equals(object obj) => obj is Error other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Code, Message);
}
