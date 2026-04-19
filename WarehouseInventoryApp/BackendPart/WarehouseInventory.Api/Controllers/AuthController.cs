using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseInventory.Api.Dtos;
using WarehouseInventory.Api.Services;

namespace WarehouseInventory.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Registers a new user and returns a JWT token for authorized requests.
    /// </summary>
    /// <param name="request">User registration data.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>JWT token and basic user information.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Response);
    }

    /// <summary>
    /// Authenticates an existing user and returns a JWT token.
    /// </summary>
    /// <param name="request">User login credentials.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>JWT token and basic user information.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = result.Error });
        }

        return Ok(result.Response);
    }
}
