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
            var problem = new DomainResultProblemDetails(errorResult.Errors)
            {
                Status = GetHttpErrorCode(errorResult.ErrorType), 
                Detail = error.ToString()
            };
            return new ObjectResult(problem);
        }
        
        return new ObjectResult(error)
        {
            StatusCode = DomainResponseExtensions.ResolveStatusCode(error)
        };
    }

    private static int GetHttpErrorCode(ErrorType errorResultErrorType)
    {
        return errorResultErrorType switch
        {
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.MissingEntity => StatusCodes.Status404NotFound,
            ErrorType.ValidationError => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status422UnprocessableEntity
        };
    }
}

internal class DomainResultProblemDetails(Error[] errors) : ProblemDetails
{
    public Error[] Errors { get; set; } = errors;
}