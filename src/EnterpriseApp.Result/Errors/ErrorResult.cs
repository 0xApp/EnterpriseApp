using System.Linq;

namespace EnterpriseApp.Errors;

public class ErrorResult(ErrorType errorType, params Error[] errors)
{
    public ErrorType ErrorType { get; } = errorType;
    public Error[] Errors { get; } = errors;

    public ErrorResult(ErrorType errorType, Error error): this(errorType, [error])
    {
        
    }

    public static ErrorResult Unauthorized(string code) => new(ErrorType.Unauthorized, code);
    public static ErrorResult Unauthorized(string code, string message) => new(ErrorType.Unauthorized, new Error(code, message));
    
    public static ErrorResult Forbidden(string code) => new(ErrorType.Forbidden, code);
    public static ErrorResult Forbidden(string code, string message) => new(ErrorType.Forbidden, new Error(code, message));
    
    public static ErrorResult NotFound(string code) => new(ErrorType.NotFound, code);
    public static ErrorResult NotFound(string code, string message) => new(ErrorType.NotFound, new Error(code, message));
    
    public static ErrorResult BusinessRuleViolation(string code) => new(ErrorType.BusinessRuleViolation, new Error(code));
    public static ErrorResult BusinessRuleViolation(string[] codes) => new(ErrorType.BusinessRuleViolation, codes.Select(c => new Error(c)).ToArray());
}
