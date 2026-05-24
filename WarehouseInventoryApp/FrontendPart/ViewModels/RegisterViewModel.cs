using WarehouseInventory.Desktop.Infrastructure;
using WarehouseInventory.Desktop.Models;
using WarehouseInventory.Desktop.Services.Api;

namespace WarehouseInventory.Desktop.ViewModels;

public sealed class RegisterViewModel : ViewModelBase
{
    private readonly Action _openLogin;
    private readonly Action<UserSessionProfile> _openWorkspace;
    private readonly AuthApiService _authApiService = new();
    private string _userName = string.Empty;
    private string _employeeFullName = string.Empty;
    private string _email = string.Empty;
    private string _phone = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _statusMessage = "Создайте аккаунт. Регистрация выполняется через API и сразу возвращает токен.";
    private bool _isBusy;

    public RegisterViewModel(Action openLogin, Action<UserSessionProfile> openWorkspace)
    {
        _openLogin = openLogin;
        _openWorkspace = openWorkspace;
        SubmitCommand = new AsyncRelayCommand(SubmitAsync, () => !IsBusy);
        OpenLoginCommand = new RelayCommand(() => _openLogin());
    }

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public string EmployeeFullName
    {
        get => _employeeFullName;
        set => SetProperty(ref _employeeFullName, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetProperty(ref _isBusy, value))
            {
                return;
            }

            SubmitCommand.RaiseCanExecuteChanged();
        }
    }

    public AsyncRelayCommand SubmitCommand { get; }

    public RelayCommand OpenLoginCommand { get; }

    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(UserName) ||
            string.IsNullOrWhiteSpace(EmployeeFullName) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Phone) ||
            string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            StatusMessage = "Заполните все поля. Регистрация любит полноту данных.";
            return;
        }

        if (!Email.Contains('@'))
        {
            StatusMessage = "Email выглядит неверно. Добавьте '@' и попробуйте снова.";
            return;
        }

        if (Password.Length < 8)
        {
            StatusMessage = "Пароль должен быть не короче 8 символов.";
            return;
        }

        if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
        {
            StatusMessage = "Пароли не совпадают.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Отправляю регистрацию в API и жду токен...";

        try
        {
            var result = await _authApiService.RegisterAsync(
                UserName.Trim(),
                EmployeeFullName.Trim(),
                Email.Trim(),
                Password,
                CancellationToken.None);

            if (!result.Succeeded || result.Profile is null)
            {
                StatusMessage = result.ErrorMessage;
                return;
            }

            StatusMessage = $"API-регистрация для {result.Profile.UserName} завершена. Открываю рабочую область.";
            _openWorkspace(result.Profile);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
