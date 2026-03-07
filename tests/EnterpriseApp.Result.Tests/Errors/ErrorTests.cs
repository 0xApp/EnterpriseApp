using EnterpriseApp.Errors;

namespace EnterpriseApp.Result.Tests.Errors;

public class ErrorTests
{
    [Fact]
    public void Constructor_WithCodeAndMessage_SetsProperties()
    {
        var error = new Error("ERR001", "Something went wrong");

        Assert.Equal("ERR001", error.Code);
        Assert.Equal("Something went wrong", error.Message);
    }

    [Fact]
    public void Constructor_WithCodeOnly_SetsMessageToEmpty()
    {
        var error = new Error("ERR001");

        Assert.Equal("ERR001", error.Code);
        Assert.Equal(string.Empty, error.Message);
    }

    [Fact]
    public void ImplicitConversion_FromString_CreatesErrorWithCode()
    {
        Error error = "ERR001";

        Assert.Equal("ERR001", error.Code);
        Assert.Equal(string.Empty, error.Message);
    }

    [Fact]
    public void Equals_SameCodeAndMessage_ReturnsTrue()
    {
        var error1 = new Error("ERR001", "msg");
        var error2 = new Error("ERR001", "msg");

        Assert.True(error1.Equals(error2));
    }

    [Fact]
    public void Equals_DifferentCode_ReturnsFalse()
    {
        var error1 = new Error("ERR001", "msg");
        var error2 = new Error("ERR002", "msg");

        Assert.False(error1.Equals(error2));
    }

    [Fact]
    public void Equals_DifferentMessage_ReturnsFalse()
    {
        var error1 = new Error("ERR001", "msg1");
        var error2 = new Error("ERR001", "msg2");

        Assert.False(error1.Equals(error2));
    }

    [Fact]
    public void Equals_WithObject_ReturnsTrueForMatchingError()
    {
        var error1 = new Error("ERR001", "msg");
        object error2 = new Error("ERR001", "msg");

        Assert.True(error1.Equals(error2));
    }

    [Fact]
    public void Equals_WithNonErrorObject_ReturnsFalse()
    {
        var error = new Error("ERR001", "msg");

        Assert.False(error.Equals("not an error"));
    }

    [Fact]
    public void GetHashCode_SameErrors_ReturnsSameHash()
    {
        var error1 = new Error("ERR001", "msg");
        var error2 = new Error("ERR001", "msg");

        Assert.Equal(error1.GetHashCode(), error2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentErrors_ReturnsDifferentHash()
    {
        var error1 = new Error("ERR001", "msg1");
        var error2 = new Error("ERR002", "msg2");

        Assert.NotEqual(error1.GetHashCode(), error2.GetHashCode());
    }
}
