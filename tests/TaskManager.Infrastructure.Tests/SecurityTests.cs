using FluentAssertions;
using Microsoft.Extensions.Configuration;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Security;

namespace TaskManager.Infrastructure.Tests;

public class SecurityTests
{
    [Fact]
    public async Task JwtTokenGenerator_GeneratesNonNullToken()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "TaskManager-Test-Key-With-At-Least-32-Chars",
            ["Jwt:Issuer"] = "TaskManager.API.Tests",
            ["Jwt:Audience"] = "TaskManager.Client.Tests"
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var generator = new JwtTokenGenerator(configuration);
        var user = new User { Id = Guid.NewGuid(), Email = "demo@ballastlane.com", Name = "Demo" };

        var result = await Task.FromResult(generator.GenerateToken(user));

        result.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task PasswordHasher_Verify_ReturnsTrue_ForCorrectPassword()
    {
        var hasher = new PasswordHasher();
        const string password = "VeryStrongPassword!123";
        var hash = hasher.HashPassword(password);

        var isValid = await Task.FromResult(hasher.VerifyPassword(password, hash));

        isValid.Should().BeTrue();
    }
}
