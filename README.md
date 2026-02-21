# EnterpriseApp

A lightweight .NET library that provides a type-safe result pattern for domain-driven error handling in enterprise applications.

## Packages

| Package | Description | Target |
|---------|-------------|--------|
| **EnterpriseApp.Result** | Core result types and error model | .NET Standard 2.1 |
| **EnterpriseApp.Result.Mvc** | ASP.NET Core integration with automatic HTTP response mapping | .NET 6.0 &ndash; 10.0 |

## Installation

```shell
dotnet add package EnterpriseApp.Result
dotnet add package EnterpriseApp.Result.Mvc
```

## Quick Start

### Returning results from a service

```csharp
using EnterpriseApp.Errors;
using EnterpriseApp.Result;

public class OrderService
{
    public Result<Order> GetOrder(int id)
    {
        var order = _repository.Find(id);
        if (order is null)
            return ErrorResult.NotFound("ORDER_NOT_FOUND", "Order does not exist.");

        return order; // implicit conversion to Result<Order>
    }
}
```

`Result<T>` wraps either a success value or an `ErrorResult`. Implicit operators let you return the value or error directly without ceremony.

### Consuming results

Use the `Match` method to handle both outcomes:

```csharp
public Result<double> GetAverageTemperature()
{
    Result<IEnumerable<WeatherForecast>> forecasts = _weatherService.Get();

    return forecasts.Match<Result<double>>(
        success => success.Average(f => f.TemperatureC),
        error   => error);
}
```

Or inspect the result directly:

```csharp
var result = service.GetOrder(42);

if (result.IsSuccess)
    Console.WriteLine(result.Value.Total);
else
    Console.WriteLine(result.ErrorResult.Errors[0].Code);
```

### ASP.NET Core controllers

Return a `DomainResponse` from your action and the library maps the result to the appropriate HTTP status code automatically:

```csharp
using EnterpriseApp.Errors;
using EnterpriseApp.Result.Mvc;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    [HttpGet("{id}")]
    public DomainResponse<Order, ErrorResult> Get(int id)
    {
        return _orderService.GetOrder(id);
    }
}
```

Success returns **200 OK** with the value. Errors are mapped to standard HTTP status codes using [RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807) Problem Details:

| ErrorType | HTTP Status |
|-----------|-------------|
| `Unauthorized` | 401 |
| `Forbidden` | 403 |
| `NotFound` | 404 |
| `BusinessRuleViolation` | 422 |
| Other | 400 |

## Error Model

### ErrorType

A set of predefined domain error categories:

`NotFound` &middot; `BusinessRuleViolation` &middot; `ValidationError` &middot; `Unauthorized` &middot; `Forbidden` &middot; `SystemError` &middot; `ServiceUnavailable` &middot; `ServiceError`

### ErrorResult

Create errors using the static factory methods:

```csharp
ErrorResult.NotFound("RESOURCE_NOT_FOUND");
ErrorResult.NotFound("RESOURCE_NOT_FOUND", "The requested resource was not found.");
ErrorResult.Unauthorized("NOT_AUTHORIZED", "Authentication is required.");
ErrorResult.Forbidden("ACCESS_DENIED", "Insufficient permissions.");
ErrorResult.BusinessRuleViolation("INSUFFICIENT_BALANCE");
ErrorResult.BusinessRuleViolation(["RULE_A", "RULE_B"]); // multiple errors
```

### Error

Each `Error` has a `Code` and an optional `Message`. Strings are implicitly converted to error codes:

```csharp
Error error = "SOME_CODE"; // implicit conversion
```

## Running the Sample

```shell
cd samples/EnterpriseApp.MvcSample
dotnet run
```

The sample API exposes Swagger UI and two endpoints that cycle through success and error responses to demonstrate the result pattern in action.

## License

[MIT](LICENSE) &copy; 2025 Parimal Raj
