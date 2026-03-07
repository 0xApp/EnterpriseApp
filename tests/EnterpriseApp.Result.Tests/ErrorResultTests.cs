using EnterpriseApp.Errors;

namespace EnterpriseApp.Result.Tests;

public class ErrorResultTests
{
    [Fact]
    public void Constructor_WithSingleError_SetsErrorTypeAndErrors()
    {
        var error = new Error("ERR001", "msg");
        var result = new ErrorResult(ErrorType.BusinessRuleViolation, error);

        Assert.Equal(ErrorType.BusinessRuleViolation, result.ErrorType);
        Assert.Single(result.Errors);
        Assert.Equal("ERR001", result.Errors[0].Code);
    }

    [Fact]
    public void Constructor_WithErrorArray_SetsAllErrors()
    {
        var errors = new[] { new Error("ERR001"), new Error("ERR002") };
        var result = new ErrorResult(ErrorType.ValidationError, errors);

        Assert.Equal(ErrorType.ValidationError, result.ErrorType);
        Assert.Equal(2, result.Errors.Length);
        Assert.Equal("ERR001", result.Errors[0].Code);
        Assert.Equal("ERR002", result.Errors[1].Code);
    }

    [Fact]
    public void Errors_Property_ReturnsSameArrayOnMultipleAccesses()
    {
        var error = new Error("ERR001");
        var result = new ErrorResult(ErrorType.MissingEntity, error);

        var first = result.Errors;
        var second = result.Errors;

        Assert.Same(first, second);
    }

    [Fact]
    public void Unauthorized_WithCode_CreatesCorrectErrorResult()
    {
        var result = ErrorResult.Unauthorized("AUTH001");

        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
        Assert.Single(result.Errors);
        Assert.Equal("AUTH001", result.Errors[0].Code);
    }

    [Fact]
    public void Unauthorized_WithCodeAndMessage_CreatesCorrectErrorResult()
    {
        var result = ErrorResult.Unauthorized("AUTH001", "Not authenticated");

        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
        Assert.Equal("AUTH001", result.Errors[0].Code);
        Assert.Equal("Not authenticated", result.Errors[0].Message);
    }

    [Fact]
    public void Forbidden_WithCode_CreatesCorrectErrorResult()
    {
        var result = ErrorResult.Forbidden("FORBID001");

        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
        Assert.Equal("FORBID001", result.Errors[0].Code);
    }

    [Fact]
    public void Forbidden_WithCodeAndMessage_CreatesCorrectErrorResult()
    {
        var result = ErrorResult.Forbidden("FORBID001", "Access denied");

        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
        Assert.Equal("FORBID001", result.Errors[0].Code);
        Assert.Equal("Access denied", result.Errors[0].Message);
    }

    [Fact]
    public void NotFound_WithCode_CreatesCorrectErrorResult()
    {
        var result = ErrorResult.NotFound("NF001");

        Assert.Equal(ErrorType.MissingEntity, result.ErrorType);
        Assert.Equal("NF001", result.Errors[0].Code);
    }

    [Fact]
    public void NotFound_WithCodeAndMessage_CreatesCorrectErrorResult()
    {
        var result = ErrorResult.NotFound("NF001", "Entity not found");

        Assert.Equal(ErrorType.MissingEntity, result.ErrorType);
        Assert.Equal("NF001", result.Errors[0].Code);
        Assert.Equal("Entity not found", result.Errors[0].Message);
    }

    [Fact]
    public void BusinessRuleViolation_WithCode_CreatesCorrectErrorResult()
    {
        var result = ErrorResult.BusinessRuleViolation("BRV001");

        Assert.Equal(ErrorType.BusinessRuleViolation, result.ErrorType);
        Assert.Equal("BRV001", result.Errors[0].Code);
    }

    [Fact]
    public void BusinessRuleViolation_WithCodes_CreatesMultipleErrors()
    {
        var result = ErrorResult.BusinessRuleViolation(new[] { "BRV001", "BRV002" });

        Assert.Equal(ErrorType.BusinessRuleViolation, result.ErrorType);
        Assert.Equal(2, result.Errors.Length);
        Assert.Equal("BRV001", result.Errors[0].Code);
        Assert.Equal("BRV002", result.Errors[1].Code);
    }
}
