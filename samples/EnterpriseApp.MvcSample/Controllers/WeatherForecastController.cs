using EnterpriseApp.Errors;
using EnterpriseApp.Result;
using EnterpriseApp.Result.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseApp.MvcSample.Controllers;

[ApiController]
[Route("weather-forecast")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet("forecast")]
    public DomainResponse<IEnumerable<WeatherForecast>, ErrorResult> GetWeatherForecast()
    {
        return ComplexService.Get();
    }
    
    [HttpGet("average-temperature")]
    public DomainResponse<double, ErrorResult> GetAverageTemperature()
    {
        return ConsumerService.GetAverageTemperature();
    }
}

internal static class ComplexService
{
    private static readonly string[] Summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];
    private static int _counter = -1;
    public static Result<IEnumerable<WeatherForecast>> Get()
    {
        _counter++;
        return (_counter % 4) switch
        {
            0 => Enumerable.Range(1, 5)
                .Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray(),
            1 => ErrorResult.BusinessRuleViolation(["BUSINESS_RULE_VIOLATION", "ANOTHER_BUSINESS_RULE_VIOLATION"]),
            2 => ErrorResult.Unauthorized("NOT_AUTHORIZED", "You are not authorized to access this resource."),
            _ => ErrorResult.Forbidden("FORBIDDEN", "You are forbidden to access this resource.")
        };
    }
}

internal static class ConsumerService
{
    public static Result<double> GetAverageTemperature()
    {
        var weatherForecastResult = ComplexService.Get();
        return weatherForecastResult.Match<Result<double>>(
            forecasts => forecasts.Average(f => f.TemperatureC), 
            result => result);
    }
}
