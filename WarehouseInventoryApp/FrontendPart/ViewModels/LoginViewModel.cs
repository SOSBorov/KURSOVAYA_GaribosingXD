using WarehouseInventory.Desktop.Infrastructure;
using WarehouseInventory.Desktop.Models;
using WarehouseInventory.Desktop.Services.Api;

namespace WarehouseInventory.Desktop.ViewModels;

public sealed class LoginViewModel : ViewModelBase
{
    private readonly Action _openRegister;
    private readonly Action<UserSessionProfile> _openWorkspace;
    private readonly AuthApiService _authApiService = new();
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _statusMessage = "Введите почту и пароль. Авторизация выполняется через API.";
    private bool _isBusy;

    public LoginViewModel(Action openRegister, Action<UserSessionProfile> openWorkspace)
    {
        _openRegister = openRegister;
        _openWorkspace = openWorkspace;
        SubmitCommand = new AsyncRelayCommand(SubmitAsync, () => !IsBusy);
        OpenRegisterCommand = new RelayCommand(() => _openRegister());
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
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

    public RelayCommand OpenRegisterCommand { get; }

    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Заполните оба поля, иначе входить пока просто некуда.";
            return;
        }

        if (!Email.Contains('@'))
        {
            StatusMessage = "Почта выглядит подозрительно. Добавьте символ '@'.";
            return;
        }

        if (Password.Length < 8)
        {
            StatusMessage = "Пароль должен быть не короче 8 символов.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Подключаюсь к API и проверяю учетные данные...";

        try
        {
            var result = await _authApiService.LoginAsync(Email.Trim(), Password, CancellationToken.None);
            if (!result.Succeeded || result.Profile is null)
            {
                StatusMessage = result.ErrorMessage;
                return;
            }

            StatusMessage = $"API-авторизация для {result.Profile.UserName} успешна. Открываю рабочую область.";
            _openWorkspace(result.Profile);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
