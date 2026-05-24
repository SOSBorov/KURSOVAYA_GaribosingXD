using System.Collections.ObjectModel;

namespace WarehouseInventory.Desktop.Models;

public sealed class InventoryAppState
{
    public InventoryAppState()
    {
        Products = new ObservableCollection<ProductItem>
        {
            new() { Name = "Сканер штрихкодов Zebra DS2208", Sku = "SCAN-2208", Category = "Электроника", Unit = "шт", Quantity = 14, ImagePath = null },
            new() { Name = "Термобумага 58 мм", Sku = "PAPER-58", Category = "Расходники", Unit = "рулон", Quantity = 42, ImagePath = null },
            new() { Name = "Маркер перманентный черный", Sku = "OFF-149", Category = "Канцелярия", Unit = "шт", Quantity = 67, ImagePath = null },
            new() { Name = "Очиститель контактов", Sku = "CHEM-011", Category = "Химия", Unit = "л", Quantity = 8, ImagePath = null }
        };
    }

    public ObservableCollection<ProductItem> Products { get; }

    public string CurrentUserName { get; set; } = "Локальный оператор";

    public string UserEmail { get; set; } = string.Empty;

    public string EmployeeFullName { get; set; } = "Локальный сотрудник";

    public string OperatorRole { get; set; } = "Старший оператор склада";

    public string Shift { get; set; } = "Дневная смена 08:00 - 20:00";

    public string WarehouseZone { get; set; } = "Зона А / Стеллажи A1-A8";

    public string AccessLevel { get; set; } = "Инвентаризация, приемка, корректировка карточек";

    public string EmployeeId { get; set; } = "WH-042";

    public string LastLogin { get; set; } = "21.04.2026 21:18";

    public int TasksCompletedToday { get; set; } = 18;

    public int InventoryAccuracyPercent { get; set; } = 98;

    public string Notes { get; set; } = "Ответственный за ежедневную сверку сканеров, термобумаги и мелкой электроники.";

    public string? ProfileImagePath { get; set; }

    public int TotalProductsInBase { get; set; } = 125;

    public string LastCheckDate { get; set; } = "12.04.2024";

    public int ActiveDocuments { get; set; } = 2;

    public bool IsAdmin { get; set; }

    public bool IsApiSession { get; set; }

    public string AuthToken { get; set; } = string.Empty;

    public DateTime? TokenExpiresAtUtc { get; set; }

    public string SessionModeDescription =>
        IsApiSession
            ? $"API-сессия активна до {TokenExpiresAtUtc?.ToLocalTime():dd.MM.yyyy HH:mm}"
            : "Локальная demo-сборка без API";

    public void ApplyUser(UserSessionProfile profile)
    {
        CurrentUserName = profile.UserName;
        UserEmail = profile.Email;
        EmployeeFullName = profile.EmployeeFullName;
        OperatorRole = profile.OperatorRole;
        Shift = profile.Shift;
        WarehouseZone = profile.WarehouseZone;
        AccessLevel = profile.AccessLevel;
        EmployeeId = profile.EmployeeId;
        LastLogin = profile.LastLogin;
        TasksCompletedToday = profile.TasksCompletedToday;
        InventoryAccuracyPercent = profile.InventoryAccuracyPercent;
        Notes = profile.Notes;
        IsAdmin = profile.IsAdmin;
        IsApiSession = profile.IsApiSession;
        AuthToken = profile.Token;
        TokenExpiresAtUtc = profile.TokenExpiresAtUtc;
    }
}
