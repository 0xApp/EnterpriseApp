using System;
using EnterpriseApp.Errors;

namespace EnterpriseApp.Result.Tests;

public class ResultTests
{
    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessResult()
    {
        Result<string> result = "hello";

        Assert.True(result.IsSuccess);
        Assert.False(result.IsError);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromErrorResult_CreatesErrorResult()
    {
        var error = ErrorResult.NotFound("NF001", "Not found");
        Result<string> result = error;

        Assert.False(result.IsSuccess);
        Assert.True(result.IsError);
        Assert.Same(error, result.ErrorResult);
    }

    [Fact]
    public void Match_OnSuccess_ReturnsSuccessValue()
    {
        Result<int> result = 42;

        var output = result.Match(
            success: v => v * 2,
            error: _ => -1);

        Assert.Equal(84, output);
    }

    [Fact]
    public void Match_OnError_ReturnsErrorValue()
    {
        Result<int> result = ErrorResult.BusinessRuleViolation("BRV001");

        var output = result.Match(
            success: v => v * 2,
            error: e => -1);

        Assert.Equal(-1, output);
    }

    [Fact]
    public void Switch_OnSuccess_ExecutesSuccessAction()
    {
        Result<string> result = "test";
        string? captured = null;

        result.Switch(
            success: v => captured = v,
            error: _ => { });

        Assert.Equal("test", captured);
    }

    [Fact]
    public void Switch_OnError_ExecutesErrorAction()
    {
        var error = ErrorResult.Forbidden("F001");
        Result<string> result = error;
        ErrorResult? captured = null;

        result.Switch(
            success: _ => { },
            error: e => captured = e);

        Assert.Same(error, captured);
    }

    [Fact]
    public void ExplicitCast_ToValue_OnSuccess_ReturnsValue()
    {
        Result<int> result = 100;

        var value = (int)result;

        Assert.Equal(100, value);
    }

    [Fact]
    public void ExplicitCast_ToValue_OnError_Throws()
    {
        Result<int> result = ErrorResult.Unauthorized("A001");

        Assert.Throws<InvalidOperationException>(() => (int)result);
    }

    [Fact]
    public void ExplicitCast_ToErrorResult_OnError_ReturnsErrorResult()
    {
        var error = ErrorResult.NotFound("NF001");
        Result<string> result = error;

        var cast = (ErrorResult)result;

        Assert.Same(error, cast);
    }

    [Fact]
    public void ExplicitCast_ToErrorResult_OnSuccess_Throws()
    {
        Result<string> result = "success";

        Assert.Throws<InvalidOperationException>(() => (ErrorResult)result);
    }

    [Fact]
    public void Match_WithErrorResult_CanInspectErrorDetails()
    {
        Result<string> result = ErrorResult.NotFound("NF001", "User not found");

        var output = result.Match(
            success: v => "ok",
            error: e =>
            {
                Assert.Equal(ErrorType.MissingEntity, e.ErrorType);
                Assert.Equal("NF001", e.Errors[0].Code);
                Assert.Equal("User not found", e.Errors[0].Message);
                return "not found";
            });

        Assert.Equal("not found", output);
    }
}
