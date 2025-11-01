using EnterpriseApp.Result;

namespace EnterpriseApp.Errors;

public class Error(string code, string message)
{
    public string Code { get; } = code;
    public string Message { get; } = message;

    public Error(string code) : this(code, string.Empty)
    {
    }
    
    public static implicit operator Error(string code) => new(code);
    
}
