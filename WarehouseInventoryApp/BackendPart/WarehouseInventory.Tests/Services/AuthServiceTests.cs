using Microsoft.AspNetCore.Identity;
using Moq;
using WarehouseInventory.Api.Dtos;
using WarehouseInventory.Api.Models;
using WarehouseInventory.Api.Services;
using WarehouseInventory.Tests.Helpers;

namespace WarehouseInventory.Tests.Services;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_ShouldCreateUserAndReturnToken()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var jwtTokenService = new Mock<IJwtTokenService>();
        var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();

        var expectedResponse = new AuthResponse
        {
            Token = "jwt-token",
            UserName = "Knight",
            Email = "user@example.com",
            ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
        };

        passwordHasher
            .Setup(hasher => hasher.HashPassword(It.IsAny<ApplicationUser>(), "StrongPass123"))
            .Returns("hashed-password");
        jwtTokenService
            .Setup(service => service.CreateToken(It.IsAny<ApplicationUser>()))
            .Returns(expectedResponse);

        var service = new AuthService(dbContext, jwtTokenService.Object, passwordHasher.Object);
        var request = new RegisterRequest
        {
            UserName = "  Knight  ",
            Email = "  USER@example.com  ",
            Password = "StrongPass123"
        };

        var result = await service.RegisterAsync(request);

        Assert.True(result.Succeeded);
        Assert.Null(result.Error);
        Assert.Equal(expectedResponse.Token, result.Response?.Token);

        var createdUser = dbContext.Users.Single();
        Assert.Equal("Knight", createdUser.UserName);
        Assert.Equal("user@example.com", createdUser.Email);
        Assert.Equal("hashed-password", createdUser.PasswordHash);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenPasswordIsInvalid()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "Knight",
            Email = "user@example.com",
            PasswordHash = "hashed-password",
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var jwtTokenService = new Mock<IJwtTokenService>(MockBehavior.Strict);
        var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
        passwordHasher
            .Setup(hasher => hasher.VerifyHashedPassword(user, "hashed-password", "WrongPass123"))
            .Returns(PasswordVerificationResult.Failed);

        var service = new AuthService(dbContext, jwtTokenService.Object, passwordHasher.Object);
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "WrongPass123"
        };

        var result = await service.LoginAsync(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.Response);
        Assert.NotNull(result.Error);
    }
}
