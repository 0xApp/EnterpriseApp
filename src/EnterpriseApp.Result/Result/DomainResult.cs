using System;
using OneOf;

namespace EnterpriseApp.Result;

[GenerateOneOf]
public class DomainResult<TResult, TError> : OneOfBase<TResult, TError>
{
    // protected DomainResult(OneOf<TResult, TError> input, TResult value, TError errorResult, bool isSuccess) : base(input)
    // {
    //     Value = value;
    //     ErrorResult = errorResult;
    //     IsSuccess = isSuccess;
    // }

    public new TResult Value { get; }

    public TError ErrorResult { get; }



    internal DomainResult(TResult value) : base(value)
    {
        Value = value;
        IsSuccess = true;
    }
    
    internal DomainResult(TError error) : base(error)
    {
        ErrorResult = error;
        IsSuccess = false;
    }
    
    public bool IsSuccess { get; }
    public bool IsError => !IsSuccess;
    
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
