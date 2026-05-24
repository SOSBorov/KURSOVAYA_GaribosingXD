using System.Collections.ObjectModel;

namespace WarehouseInventory.Desktop.Models;

public sealed class InventoryAppState
{
    public InventoryAppState()
    {
        Products = new ObservableCollection<ProductItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Сканер штрихкодов Zebra DS2208", Sku = "SCAN-2208", Category = "Электроника", Unit = "шт", Quantity = 14, WarehouseLocation = "A-01", UnitPrice = 12990m },
            new() { Id = Guid.NewGuid(), Name = "Термобумага 58 мм", Sku = "PAPER-58", Category = "Расходники", Unit = "рулон", Quantity = 42, WarehouseLocation = "B-03", UnitPrice = 180m },
            new() { Id = Guid.NewGuid(), Name = "Маркер перманентный черный", Sku = "OFF-149", Category = "Канцелярия", Unit = "шт", Quantity = 67, WarehouseLocation = "C-11", UnitPrice = 95m },
            new() { Id = Guid.NewGuid(), Name = "Очиститель контактов", Sku = "CHEM-011", Category = "Химия", Unit = "л", Quantity = 8, WarehouseLocation = "D-07", UnitPrice = 420m }
        };

        InventoryChecks = new ObservableCollection<InventoryCheckItem>();
        TotalProductsInBase = Products.Count;
    }

    public ObservableCollection<ProductItem> Products { get; }

    public ObservableCollection<InventoryCheckItem> InventoryChecks { get; }

    public string CurrentUserName { get; set; } = "Локальный оператор";

    public string UserEmail { get; set; } = string.Empty;

    public string EmployeeFullName { get; set; } = "Сотрудник склада";

    public string OperatorRole { get; set; } = "Старший оператор склада";

    public string Shift { get; set; } = "Дневная смена 08:00 - 20:00";

    public string WarehouseZone { get; set; } = "Зона A / Стеллажи A1-A8";

    public string AccessLevel { get; set; } = "Инвентаризация, приемка, корректировка карточек";

    public string EmployeeId { get; set; } = "WH-042";

    public string LastLogin { get; set; } = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

    public int TasksCompletedToday { get; set; } = 18;

    public int InventoryAccuracyPercent { get; set; } = 98;

    public string Notes { get; set; } = "Отвечает за ежедневную сверку и проверку остатков в зоне хранения.";

    public string? ProfileImagePath { get; set; }

    public int TotalProductsInBase { get; set; }

    public string LastCheckDate { get; set; } = DateTime.Today.ToString("dd.MM.yyyy");

    public int ActiveDocuments { get; set; } = 2;

    public DateTime? InventorySavedAtUtc { get; set; }

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
