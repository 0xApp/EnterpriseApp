using EnterpriseApp.Result.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseApp.Result.Mvc.Tests.Mvc;

public class DomainResponseExtensionsTests
{
    [Fact]
    public void MapErrorResult_WithMapper_RegistersCustomMapper()
    {
        var services = new ServiceCollection();

        services.MapErrorResult<string>(error => error switch
        {
            "not_found" => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        });

        DomainResult<int, string> result = "not_found";
        DomainResponse<int, string> response = result;

        var actionResult = response.Convert();

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
    }

    [Fact]
    public void MapErrorResult_WithConstantCode_RegistersConstantMapper()
    {
        var services = new ServiceCollection();

        services.MapErrorResult<int>(StatusCodes.Status503ServiceUnavailable);

        DomainResult<string, int> result = 42;
        DomainResponse<string, int> response = result;

        var actionResult = response.Convert();

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, objectResult.StatusCode);
    }

    [Fact]
    public void MapErrorResult_WithMapper_ReturnsServiceCollection()
    {
        var services = new ServiceCollection();

        var returned = services.MapErrorResult<string>(_ => 400);

        Assert.Same(services, returned);
    }

    [Fact]
    public void MapErrorResult_WithConstantCode_ReturnsServiceCollection()
    {
        var services = new ServiceCollection();

        var returned = services.MapErrorResult<double>(400);

        Assert.Same(services, returned);
    }

    [Fact]
    public void Convert_WithUnregisteredErrorType_Returns422()
    {
        DomainResult<string, bool> result = true;
        DomainResponse<string, bool> response = result;

        var actionResult = response.Convert();

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);
    }
}
