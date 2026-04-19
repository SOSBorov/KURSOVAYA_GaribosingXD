using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Api.Data;
using WarehouseInventory.Api.Dtos;
using WarehouseInventory.Api.Models;

namespace WarehouseInventory.Api.Services;

public sealed class AuthService(
    ApplicationDbContext dbContext,
    IJwtTokenService jwtTokenService,
    IPasswordHasher<ApplicationUser> passwordHasher) : IAuthService
{
    public async Task<(bool Succeeded, string? Error, AuthResponse? Response)> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedUserName = request.UserName.Trim();

        var emailExists = await dbContext.Users
            .AnyAsync(user => user.Email == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            return (false, "Пользователь с таким email уже существует.", null);
        }

        var userNameExists = await dbContext.Users
            .AnyAsync(user => user.UserName == normalizedUserName, cancellationToken);

        if (userNameExists)
        {
            return (false, "Пользователь с таким именем уже существует.", null);
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = normalizedUserName,
            Email = normalizedEmail,
            CreatedAtUtc = DateTime.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return (true, null, jwtTokenService.CreateToken(user));
    }

    public async Task<(bool Succeeded, string? Error, AuthResponse? Response)> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(currentUser => currentUser.Email == normalizedEmail, cancellationToken);

        if (user is null)
        {
            return (false, "Неверный email или пароль.", null);
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return (false, "Неверный email или пароль.", null);
        }

        return (true, null, jwtTokenService.CreateToken(user));
    }
}
