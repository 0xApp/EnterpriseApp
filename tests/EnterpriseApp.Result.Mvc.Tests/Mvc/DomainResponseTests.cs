using EnterpriseApp.Errors;
using EnterpriseApp.Result.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseApp.Result.Mvc.Tests.Mvc;

public class DomainResponseTests
{
    [Fact]
    public void Convert_OnSuccess_ReturnsOkObjectResult()
    {
        Result<string> result = "hello";
        DomainResponse<string, ErrorResult> response = result;

        var actionResult = response.Convert();

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal("hello", okResult.Value);
    }

    [Fact]
    public void Convert_OnUnauthorizedError_Returns401()
    {
        Result<string> result = ErrorResult.Unauthorized("AUTH001", "Not authenticated");
        DomainResponse<string, ErrorResult> response = result;

        var actionResult = response.Convert();

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<DomainResultProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails.Status);
    }

    [Fact]
    public void Convert_OnForbiddenError_Returns403()
    {
        Result<string> result = ErrorResult.Forbidden("FORBID001", "Access denied");
        DomainResponse<string, ErrorResult> response = result;

        var actionResult = response.Convert();

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<DomainResultProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.Status);
    }

    [Fact]
    public void Convert_OnNotFoundError_Returns404()
    {
        Result<string> result = ErrorResult.NotFound("NF001", "Entity not found");
        DomainResponse<string, ErrorResult> response = result;

        var actionResult = response.Convert();

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<DomainResultProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
    }

    [Fact]
    public void Convert_OnValidationError_Returns400()
    {
        var errors = new[] { new Error("VAL001", "Field required") };
        var errorResult = new ErrorResult(ErrorType.ValidationError, errors);
        Result<string> result = errorResult;
        DomainResponse<string, ErrorResult> response = result;

        var actionResult = response.Convert();

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<DomainResultProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
    }

    [Fact]
    public void Convert_OnBusinessRuleViolation_Returns422()
    {
        Result<string> result = ErrorResult.BusinessRuleViolation("BRV001");
        DomainResponse<string, ErrorResult> response = result;

        var actionResult = response.Convert();

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<DomainResultProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails.Status);
    }

    [Fact]
    public void Convert_OnErrorResult_ReturnsProblemDetailsWithErrors()
    {
        Result<string> result = ErrorResult.NotFound("NF001", "User not found");
        DomainResponse<string, ErrorResult> response = result;

        var actionResult = response.Convert();

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<DomainResultProblemDetails>(objectResult.Value);
        Assert.Single(problemDetails.Errors);
        Assert.Equal("NF001", problemDetails.Errors[0].Code);
        Assert.Equal("User not found", problemDetails.Errors[0].Message);
    }

    [Fact]
    public void Convert_OnErrorResult_WithMultipleErrors_AllErrorsPresent()
    {
        var errorResult = ErrorResult.BusinessRuleViolation(new[] { "BRV001", "BRV002" });
        Result<string> result = errorResult;
        DomainResponse<string, ErrorResult> response = result;

        var actionResult = response.Convert();

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<DomainResultProblemDetails>(objectResult.Value);
        Assert.Equal(2, problemDetails.Errors.Length);
        Assert.Equal("BRV001", problemDetails.Errors[0].Code);
        Assert.Equal("BRV002", problemDetails.Errors[1].Code);
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesDomainResponse()
    {
        DomainResponse<string, ErrorResult> response = "hello";

        var actionResult = response.Convert();

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal("hello", okResult.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesDomainResponse()
    {
        DomainResponse<string, ErrorResult> response = ErrorResult.NotFound("NF001");

        var actionResult = response.Convert();

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<DomainResultProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
    }

    [Fact]
    public void Convert_OnSystemError_Returns422AsDefault()
    {
        var errorResult = new ErrorResult(ErrorType.SystemError, new Error("SYS001"));
        Result<string> result = errorResult;
        DomainResponse<string, ErrorResult> response = result;

        var actionResult = response.Convert();

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<DomainResultProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails.Status);
    }
}
