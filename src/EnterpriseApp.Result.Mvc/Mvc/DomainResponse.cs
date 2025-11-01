using EnterpriseApp.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace EnterpriseApp.Result.Mvc;

public class DomainResponse<TResult, TError>(DomainResult<TResult, TError> result) : IConvertToActionResult
{
    public static implicit operator DomainResponse<TResult, TError>(DomainResult<TResult, TError> inner) => new(inner);
    public static implicit operator DomainResponse<TResult, TError>(TResult inner) => new(inner);
    public static implicit operator DomainResponse<TResult, TError>(TError error) => new(error);

    public IActionResult Convert()
        => result.IsSuccess ? new OkObjectResult(result.Value) : MapError(result.ErrorResult);

    private static ObjectResult MapError(TError error)
    {
        if (error is ErrorResult errorResult)
        {
            var status = GetHttpErrorCode(errorResult.ErrorType);
            var problem = new DomainResultProblemDetails(errorResult.Errors)
                { Status = status, Title = "Domain error", Detail = error?.ToString() };
            return new ObjectResult(problem);
        }
        else
        {
            var status = StatusCodes.Status400BadRequest;
            var problem = new ProblemDetails { Status = status, Title = "Domain error", Detail = error?.ToString() };
            return new ObjectResult(problem);
        }
    }

    private static int GetHttpErrorCode(ErrorType errorResultErrorType)
    {
        return errorResultErrorType switch
        {
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.BusinessRuleViolation => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };
    }
}

internal class DomainResultProblemDetails(Error[] errors) : ProblemDetails
{
    public Error[] Errors { get; set; } = errors;
}