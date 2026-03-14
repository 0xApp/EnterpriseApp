using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace EnterpriseApp.Result.Mvc.Tests.Integration;

public class DomainResponseIntegrationTests : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IHost _host;
    private readonly HttpClient _client;

    public DomainResponseIntegrationTests()
    {
        _host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .UseStartup<TestStartup>();
            })
            .Start();

        _client = _host.GetTestClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _host.Dispose();
    }

    [Fact]
    public async Task Success_Returns200WithValue()
    {
        var response = await _client.GetAsync("/test/success", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("hello", content);
    }

    [Fact]
    public async Task NotFound_Returns404WithProblemDetails()
    {
        var response = await _client.GetAsync("/test/not-found", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var json = await DeserializeResponse(response);
        Assert.Equal(404, json.GetProperty("status").GetInt32());
        var errors = json.GetProperty("errors");
        Assert.Equal(1, errors.GetArrayLength());
        Assert.Equal("NF001", errors[0].GetProperty("code").GetString());
        Assert.Equal("Entity not found", errors[0].GetProperty("message").GetString());
    }

    [Fact]
    public async Task Unauthorized_Returns401WithProblemDetails()
    {
        var response = await _client.GetAsync("/test/unauthorized", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var json = await DeserializeResponse(response);
        Assert.Equal(401, json.GetProperty("status").GetInt32());
        var errors = json.GetProperty("errors");
        Assert.Equal("AUTH001", errors[0].GetProperty("code").GetString());
    }

    [Fact]
    public async Task Forbidden_Returns403WithProblemDetails()
    {
        var response = await _client.GetAsync("/test/forbidden", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var json = await DeserializeResponse(response);
        Assert.Equal(403, json.GetProperty("status").GetInt32());
        var errors = json.GetProperty("errors");
        Assert.Equal("FORBID001", errors[0].GetProperty("code").GetString());
    }

    [Fact]
    public async Task ValidationError_Returns400WithMultipleErrors()
    {
        var response = await _client.GetAsync("/test/validation-error", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await DeserializeResponse(response);
        Assert.Equal(400, json.GetProperty("status").GetInt32());
        var errors = json.GetProperty("errors");
        Assert.Equal(2, errors.GetArrayLength());
        Assert.Equal("VAL001", errors[0].GetProperty("code").GetString());
        Assert.Equal("VAL002", errors[1].GetProperty("code").GetString());
    }

    [Fact]
    public async Task BusinessRuleViolation_Returns422WithProblemDetails()
    {
        var response = await _client.GetAsync("/test/business-rule", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var json = await DeserializeResponse(response);
        Assert.Equal(422, json.GetProperty("status").GetInt32());
        var errors = json.GetProperty("errors");
        Assert.Equal("BRV001", errors[0].GetProperty("code").GetString());
    }

    private static async Task<JsonElement> DeserializeResponse(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        return JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
    }
}
