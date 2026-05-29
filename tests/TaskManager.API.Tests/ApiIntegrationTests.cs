using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TaskManager.Application.DTOs.Auth;

namespace TaskManager.API.Tests;

public class ApiIntegrationTests : IClassFixture<TaskManagerApiFactory>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(TaskManagerApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TasksGet_WithoutJwt_Returns401()
    {
        var response = await _client.GetAsync("/api/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TasksGet_WithValidJwt_Returns200AndTasksList()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().StartWith("{");
    }

    [Fact]
    public async Task AuthRegister_AllowsAnonymousAccess()
    {
        var request = new
        {
            Email = $"new-{Guid.NewGuid():N}@ballastlane.com",
            Password = "Secret123!",
            Name = "New User"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task ValidationFailures_Return400BadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "bad-email",
            Password = "123",
            Name = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<string> RegisterAndLoginAsync()
    {
        var email = $"api-user-{Guid.NewGuid():N}@ballastlane.com";
        var password = "Secret123!";
        var register = new { Email = email, Password = password, Name = "API User" };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", register);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();
        auth!.Token.Should().NotBeNullOrWhiteSpace();
        return auth.Token;
    }
}

public class TaskManagerApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"taskmanager-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_dbPath}",
                ["Jwt:Key"] = "TaskManager-Tests-Super-Secret-Key-At-Least-32-Chars",
                ["Jwt:Issuer"] = "TaskManager.API.Tests",
                ["Jwt:Audience"] = "TaskManager.Client.Tests"
            };
            configBuilder.AddInMemoryCollection(overrides);
        });
    }
}
