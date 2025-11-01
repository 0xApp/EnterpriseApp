using EnterpriseApp.Errors;

namespace EnterpriseApp.Result;

public class Result<TResult> : DomainResult<TResult, ErrorResult>
{
    internal Result(TResult value) : base(value)
    {
    }

    internal Result(ErrorResult error) : base(error)
    {
    }
    
    public static implicit operator Result<TResult>(TResult value) => new(value);
    public static implicit operator Result<TResult>(ErrorResult error) => new(error);
}
