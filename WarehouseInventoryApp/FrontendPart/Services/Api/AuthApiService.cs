using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using WarehouseInventory.Desktop.Models;

namespace WarehouseInventory.Desktop.Services.Api;

public sealed class AuthApiService
{
    private const string BaseAddress = "http://localhost:5267/";
    private readonly HttpClient _httpClient;

    public AuthApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseAddress)
        };
    }

    public async Task<AuthApiResult> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/auth/login",
                new LoginApiRequest
                {
                    Email = email,
                    Password = password
                },
                cancellationToken);

            return await HandleAuthResponseAsync(response, ExtractEmployeeName(email), cancellationToken);
        }
        catch (HttpRequestException)
        {
            return AuthApiResult.Failure("Не удалось подключиться к API. Проверьте, что backend запущен на http://localhost:5267.");
        }
        catch (TaskCanceledException)
        {
            return AuthApiResult.Failure("Запрос к API был прерван. Попробуйте еще раз.");
        }
    }

    public async Task<AuthApiResult> RegisterAsync(
        string userName,
        string employeeFullName,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/auth/register",
                new RegisterApiRequest
                {
                    UserName = userName,
                    Email = email,
                    Password = password
                },
                cancellationToken);

            return await HandleAuthResponseAsync(response, employeeFullName, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return AuthApiResult.Failure("Не удалось подключиться к API. Проверьте, что backend запущен на http://localhost:5267.");
        }
        catch (TaskCanceledException)
        {
            return AuthApiResult.Failure("Запрос к API был прерван. Попробуйте еще раз.");
        }
    }

    private static async Task<AuthApiResult> HandleAuthResponseAsync(
        HttpResponseMessage response,
        string employeeFullName,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var authResponse = await response.Content.ReadFromJsonAsync<AuthApiResponse>(cancellationToken: cancellationToken);
            if (authResponse is null || string.IsNullOrWhiteSpace(authResponse.Token))
            {
                return AuthApiResult.Failure("API вернул пустой ответ авторизации.");
            }

            var profile = UserSessionProfile.CreateApiUser(
                authResponse.UserName,
                authResponse.Email,
                string.IsNullOrWhiteSpace(employeeFullName) ? authResponse.UserName : employeeFullName,
                authResponse.Token,
                authResponse.ExpiresAtUtc);

            return AuthApiResult.Success(profile);
        }

        var errorMessage = await TryReadErrorAsync(response, cancellationToken);

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => AuthApiResult.Failure($"Авторизация отклонена: {errorMessage}"),
            HttpStatusCode.BadRequest => AuthApiResult.Failure($"Некорректные данные: {errorMessage}"),
            _ => AuthApiResult.Failure($"API вернул ошибку {(int)response.StatusCode}: {errorMessage}")
        };
    }

    private static async Task<string> TryReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(cancellationToken: cancellationToken);
            if (!string.IsNullOrWhiteSpace(error?.Message))
            {
                return error.Message;
            }
        }
        catch
        {
        }

        var rawText = await response.Content.ReadAsStringAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(rawText) ? "без деталей" : rawText;
    }

    private static string ExtractEmployeeName(string email)
    {
        var localPart = email.Split('@', StringSplitOptions.RemoveEmptyEntries)[0];
        var words = localPart
            .Split(new[] { '.', '_', '-' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(Capitalize);

        var name = string.Join(' ', words);
        return string.IsNullOrWhiteSpace(name) ? "Сотрудник API" : name;
    }

    private static string Capitalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
    }

    private sealed class LoginApiRequest
    {
        public string Email { get; init; } = string.Empty;

        public string Password { get; init; } = string.Empty;
    }

    private sealed class RegisterApiRequest
    {
        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string Password { get; init; } = string.Empty;
    }

    private sealed class AuthApiResponse
    {
        public string Token { get; init; } = string.Empty;

        public DateTime ExpiresAtUtc { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;
    }

    private sealed class ApiErrorResponse
    {
        public string Message { get; init; } = string.Empty;
    }
}
