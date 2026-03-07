namespace EnterpriseApp.Result.Tests;

public class DomainResultTests
{
    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessResult()
    {
        DomainResult<int, string> result = 42;

        Assert.True(result.IsSuccess);
        Assert.False(result.IsError);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesErrorResult()
    {
        DomainResult<int, string> result = "error message";

        Assert.False(result.IsSuccess);
        Assert.True(result.IsError);
        Assert.Equal("error message", result.ErrorResult);
    }

    [Fact]
    public void Match_OnSuccess_CallsSuccessFunc()
    {
        DomainResult<int, string> result = 42;

        var output = result.Match(
            success: v => $"Value: {v}",
            error: e => $"Error: {e}");

        Assert.Equal("Value: 42", output);
    }

    [Fact]
    public void Match_OnError_CallsErrorFunc()
    {
        DomainResult<int, string> result = "failed";

        var output = result.Match(
            success: v => $"Value: {v}",
            error: e => $"Error: {e}");

        Assert.Equal("Error: failed", output);
    }

    [Fact]
    public void Switch_OnSuccess_CallsSuccessAction()
    {
        DomainResult<int, string> result = 42;
        int? captured = null;

        result.Switch(
            success: v => captured = v,
            error: _ => { });

        Assert.Equal(42, captured);
    }

    [Fact]
    public void Switch_OnError_CallsErrorAction()
    {
        DomainResult<int, string> result = "failed";
        string? captured = null;

        result.Switch(
            success: _ => { },
            error: e => captured = e);

        Assert.Equal("failed", captured);
    }

    [Fact]
    public void ExplicitCast_ToValue_OnSuccess_ReturnsValue()
    {
        DomainResult<int, string> result = 42;

        var value = (int)result;

        Assert.Equal(42, value);
    }

    [Fact]
    public void ExplicitCast_ToValue_OnError_ThrowsInvalidOperationException()
    {
        DomainResult<int, string> result = "failed";

        Assert.Throws<InvalidOperationException>(() => (int)result);
    }

    [Fact]
    public void ExplicitCast_ToError_OnError_ReturnsError()
    {
        DomainResult<int, string> result = "failed";

        var error = (string)result;

        Assert.Equal("failed", error);
    }

    [Fact]
    public void ExplicitCast_ToError_OnSuccess_ThrowsInvalidOperationException()
    {
        DomainResult<int, string> result = 42;

        Assert.Throws<InvalidOperationException>(() => (string)result);
    }
}
