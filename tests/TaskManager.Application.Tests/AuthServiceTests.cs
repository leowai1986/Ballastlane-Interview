using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Repositories;

namespace TaskManager.Application.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task Register_HashesPassword_WithPasswordHasher()
    {
        var userRepoMock = new Mock<IUserRepository>();
        var hasherMock = new Mock<IPasswordHasher>();
        var jwtMock = new Mock<IJwtTokenGenerator>();
        var request = new RegisterRequest { Email = "demo@ballastlane.com", Password = "Secret123!", Name = "Demo" };

        userRepoMock.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        hasherMock.Setup(x => x.HashPassword(request.Password)).Returns("bcryptHash");
        userRepoMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);
        jwtMock.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(("jwt-token", DateTime.UtcNow.AddHours(1)));

        var sut = new AuthService(userRepoMock.Object, hasherMock.Object, jwtMock.Object);

        await sut.RegisterAsync(request);

        hasherMock.Verify(x => x.HashPassword(request.Password), Times.Once);
    }

    [Fact]
    public async Task Login_ReturnsAuthResponse_WithJwt_WhenCredentialsValid()
    {
        var userRepoMock = new Mock<IUserRepository>();
        var hasherMock = new Mock<IPasswordHasher>();
        var jwtMock = new Mock<IJwtTokenGenerator>();
        var user = new User { Id = Guid.NewGuid(), Email = "demo@ballastlane.com", Name = "Demo", PasswordHash = "hash" };
        var request = new LoginRequest { Email = user.Email, Password = "Secret123!" };

        userRepoMock.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        hasherMock.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash)).Returns(true);
        jwtMock.Setup(x => x.GenerateToken(user)).Returns(("token-123", DateTime.UtcNow.AddHours(2)));

        var sut = new AuthService(userRepoMock.Object, hasherMock.Object, jwtMock.Object);

        var response = await sut.LoginAsync(request);

        response.Token.Should().Be("token-123");
        response.UserId.Should().Be(user.Id);
        response.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task Login_ThrowsException_WhenPasswordWrong()
    {
        var userRepoMock = new Mock<IUserRepository>();
        var hasherMock = new Mock<IPasswordHasher>();
        var jwtMock = new Mock<IJwtTokenGenerator>();
        var user = new User { Email = "demo@ballastlane.com", PasswordHash = "hash" };
        var request = new LoginRequest { Email = user.Email, Password = "wrong" };

        userRepoMock.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        hasherMock.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash)).Returns(false);

        var sut = new AuthService(userRepoMock.Object, hasherMock.Object, jwtMock.Object);

        var act = async () => await sut.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
