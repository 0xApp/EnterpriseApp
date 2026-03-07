using System;

namespace EnterpriseApp.Result;

public class DomainResult<TResult, TError>
{
    public TResult Value { get; }

    public TError ErrorResult { get; }

    internal DomainResult(TResult value)
    {
        Value = value;
        IsSuccess = true;
    }

    internal DomainResult(TError error)
    {
        ErrorResult = error;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }
    public bool IsError => !IsSuccess;

    public TOut Match<TOut>(Func<TResult, TOut> success, Func<TError, TOut> error)
    {
        return IsSuccess ? success(Value) : error(ErrorResult);
    }

    public void Switch(Action<TResult> success, Action<TError> error)
    {
        if (IsSuccess)
            success(Value);
        else
            error(ErrorResult);
    }

    public static explicit operator TResult(DomainResult<TResult, TError> result)
    {
        return result.IsSuccess ? result.Value : throw new InvalidOperationException("Cannot cast to TResult when DomainResult is an error.");
    }

    public static explicit operator TError(DomainResult<TResult, TError> result)
    {
        return !result.IsSuccess ? result.ErrorResult : throw new InvalidOperationException("Cannot cast to TError when DomainResult is a success.");
    }

    public static implicit operator DomainResult<TResult, TError>(TResult value) => new(value);
    public static implicit operator DomainResult<TResult, TError>(TError error) => new(error);
}
