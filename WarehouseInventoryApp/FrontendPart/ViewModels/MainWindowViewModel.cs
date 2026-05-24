using WarehouseInventory.Desktop.Infrastructure;
using WarehouseInventory.Desktop.Models;

namespace WarehouseInventory.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentScreen;

    public MainWindowViewModel(Action<UserSessionProfile> openWorkspace)
    {
        Login = new LoginViewModel(ShowRegister, openWorkspace);
        Register = new RegisterViewModel(ShowLogin, openWorkspace);
        _currentScreen = Login;

        ShowLoginCommand = new RelayCommand(ShowLogin);
        ShowRegisterCommand = new RelayCommand(ShowRegister);
    }

    public LoginViewModel Login { get; }

    public RegisterViewModel Register { get; }

    public ViewModelBase CurrentScreen
    {
        get => _currentScreen;
        private set => SetProperty(ref _currentScreen, value);
    }

    public RelayCommand ShowLoginCommand { get; }

    public RelayCommand ShowRegisterCommand { get; }

    private void ShowLogin() => CurrentScreen = Login;

    private void ShowRegister() => CurrentScreen = Register;
}
