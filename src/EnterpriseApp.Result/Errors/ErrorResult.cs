using System;

namespace EnterpriseApp.Errors;

public class ErrorResult
{
    public ErrorType ErrorType { get; }

    private readonly Error _error;
    private Error[] _errors;

    public Error[] Errors
    {
        get
        {
            _errors ??= new[] { _error };
            return _errors;
        }
    }

    public ErrorResult(ErrorType errorType, Error error)
    {
        ErrorType = errorType;
        _error = error;
    }

    public ErrorResult(ErrorType errorType, Error[] errors)
    {
        ErrorType = errorType;
        _errors = errors;
        _error = errors.Length > 0 ? errors[0] : default;
    }

    public static ErrorResult Unauthorized(string code) => new(ErrorType.Unauthorized, code);
    public static ErrorResult Unauthorized(string code, string message) => new(ErrorType.Unauthorized, new Error(code, message));

    public static ErrorResult Forbidden(string code) => new(ErrorType.Forbidden, code);
    public static ErrorResult Forbidden(string code, string message) => new(ErrorType.Forbidden, new Error(code, message));

    public static ErrorResult NotFound(string code) => new(ErrorType.MissingEntity, code);
    public static ErrorResult NotFound(string code, string message) => new(ErrorType.MissingEntity, new Error(code, message));

    public static ErrorResult BusinessRuleViolation(string code) => new(ErrorType.BusinessRuleViolation, new Error(code));
    public static ErrorResult BusinessRuleViolation(string[] codes) => new(ErrorType.BusinessRuleViolation, Array.ConvertAll(codes, c => new Error(c)));
}
