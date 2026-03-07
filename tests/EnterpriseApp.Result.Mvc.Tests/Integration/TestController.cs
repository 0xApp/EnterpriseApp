using EnterpriseApp.Errors;
using EnterpriseApp.Result;
using EnterpriseApp.Result.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseApp.Result.Mvc.Tests.Integration;

[ApiController]
[Route("test")]
public class TestController : ControllerBase
{
    [HttpGet("success")]
    public DomainResponse<string, ErrorResult> GetSuccess() => "hello";

    [HttpGet("not-found")]
    public DomainResponse<string, ErrorResult> GetNotFound()
        => ErrorResult.NotFound("NF001", "Entity not found");

    [HttpGet("unauthorized")]
    public DomainResponse<string, ErrorResult> GetUnauthorized()
        => ErrorResult.Unauthorized("AUTH001", "Not authenticated");

    [HttpGet("forbidden")]
    public DomainResponse<string, ErrorResult> GetForbidden()
        => ErrorResult.Forbidden("FORBID001", "Access denied");

    [HttpGet("validation-error")]
    public DomainResponse<string, ErrorResult> GetValidationError()
    {
        var errorResult = new ErrorResult(ErrorType.ValidationError,
            new[] { new Error("VAL001", "Field required"), new Error("VAL002", "Invalid format") });
        return errorResult;
    }

    [HttpGet("business-rule")]
    public DomainResponse<string, ErrorResult> GetBusinessRule()
        => ErrorResult.BusinessRuleViolation("BRV001");
}
