namespace WarehouseInventory.Desktop.Models;

public sealed class UserSessionProfile
{
    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string EmployeeFullName { get; init; } = string.Empty;

    public string OperatorRole { get; init; } = string.Empty;

    public string Shift { get; init; } = string.Empty;

    public string WarehouseZone { get; init; } = string.Empty;

    public string AccessLevel { get; init; } = string.Empty;

    public string EmployeeId { get; init; } = string.Empty;

    public string LastLogin { get; init; } = string.Empty;

    public int TasksCompletedToday { get; init; }

    public int InventoryAccuracyPercent { get; init; }

    public string Notes { get; init; } = string.Empty;

    public bool IsAdmin { get; init; }

    public bool IsApiSession { get; init; }

    public string Token { get; init; } = string.Empty;

    public DateTime? TokenExpiresAtUtc { get; init; }

    public static UserSessionProfile CreateAdmin(string userName, string employeeFullName) =>
        new()
        {
            UserName = userName,
            Email = "gari.brous@maxpovezet.local",
            EmployeeFullName = employeeFullName,
            OperatorRole = "Администратор склада",
            Shift = "Гибкий график / контроль всех смен",
            WarehouseZone = "Все зоны склада и служебный контур",
            AccessLevel = "Полный доступ: товары, удаление, документы, поставки и профиль",
            EmployeeId = "ADM-001",
            LastLogin = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
            TasksCompletedToday = 27,
            InventoryAccuracyPercent = 99,
            Notes = "Контролирует инвентаризацию, утверждает корректировки и отвечает за критичные изменения в базе.",
            IsAdmin = true
        };

    public static UserSessionProfile CreateOperator(string userName, string employeeFullName) =>
        new()
        {
            UserName = userName,
            Email = string.Empty,
            EmployeeFullName = employeeFullName,
            OperatorRole = "Оператор склада",
            Shift = "Дневная смена 08:00 - 20:00",
            WarehouseZone = "Зона А / Стеллажи A1-A8",
            AccessLevel = "Просмотр, добавление и редактирование карточек без удаления",
            EmployeeId = $"WH-{Math.Abs(employeeFullName.GetHashCode()) % 900 + 100}",
            LastLogin = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
            TasksCompletedToday = 18,
            InventoryAccuracyPercent = 98,
            Notes = "Отвечает за приемку, локальную сверку карточек и ежедневную работу с товарами.",
            IsAdmin = false
        };

    public static UserSessionProfile CreateApiUser(
        string userName,
        string email,
        string employeeFullName,
        string token,
        DateTime expiresAtUtc)
    {
        var baseProfile = CreateOperator(userName, employeeFullName);

        return new UserSessionProfile
        {
            UserName = userName,
            Email = email,
            EmployeeFullName = employeeFullName,
            OperatorRole = "Авторизованный сотрудник склада",
            Shift = baseProfile.Shift,
            WarehouseZone = baseProfile.WarehouseZone,
            AccessLevel = "Доступ через API: чтение и изменение данных в пределах выданного токена",
            EmployeeId = baseProfile.EmployeeId,
            LastLogin = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
            TasksCompletedToday = baseProfile.TasksCompletedToday,
            InventoryAccuracyPercent = baseProfile.InventoryAccuracyPercent,
            Notes = "Профиль загружен из клиентской авторизации. После подключения серверного профиля эти данные можно будет расширить.",
            IsAdmin = string.Equals(userName, "admin", StringComparison.OrdinalIgnoreCase),
            IsApiSession = true,
            Token = token,
            TokenExpiresAtUtc = expiresAtUtc
        };
    }
}
