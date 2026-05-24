using System.IO;
using System.Text.Json;
using WarehouseInventory.Desktop.Models;

namespace WarehouseInventory.Desktop.Services.Api;

public sealed class AuthSessionStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _sessionFilePath;

    public AuthSessionStorage()
    {
        var sessionDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MaxPovezet",
            "Auth");

        Directory.CreateDirectory(sessionDirectory);
        _sessionFilePath = Path.Combine(sessionDirectory, "session.json");
    }

    public void Save(UserSessionProfile profile)
    {
        if (!profile.IsApiSession || string.IsNullOrWhiteSpace(profile.Token) || profile.TokenExpiresAtUtc is null)
        {
            return;
        }

        var record = new AuthSessionRecord
        {
            Token = profile.Token,
            ExpiresAtUtc = profile.TokenExpiresAtUtc.Value,
            UserName = profile.UserName,
            Email = profile.Email,
            EmployeeFullName = profile.EmployeeFullName
        };

        File.WriteAllText(_sessionFilePath, JsonSerializer.Serialize(record, JsonOptions));
    }

    public AuthSessionRecord? TryLoad()
    {
        if (!File.Exists(_sessionFilePath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(_sessionFilePath);
            return JsonSerializer.Deserialize<AuthSessionRecord>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public void Clear()
    {
        if (File.Exists(_sessionFilePath))
        {
            File.Delete(_sessionFilePath);
        }
    }
}
