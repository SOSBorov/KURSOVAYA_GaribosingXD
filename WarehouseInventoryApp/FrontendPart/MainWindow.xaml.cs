using System.Windows;
using WarehouseInventory.Desktop.Models;
using WarehouseInventory.Desktop.Services.Api;
using WarehouseInventory.Desktop.ViewModels;

namespace WarehouseInventory.Desktop;

public partial class MainWindow : Window
{
    private readonly InventoryAppState _appState = new();
    private readonly AuthSessionStorage _sessionStorage = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(OpenWorkspaceWindow);
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var savedSession = _sessionStorage.TryLoad();
        if (savedSession is null)
        {
            return;
        }

        if (savedSession.ExpiresAtUtc <= DateTime.UtcNow)
        {
            _sessionStorage.Clear();
            return;
        }

        var profile = UserSessionProfile.CreateApiUser(
            savedSession.UserName,
            savedSession.Email,
            savedSession.EmployeeFullName,
            savedSession.Token,
            savedSession.ExpiresAtUtc);

        OpenWorkspaceWindow(profile);
    }

    private void OpenWorkspaceWindow(UserSessionProfile profile)
    {
        _appState.ApplyUser(profile);
        if (profile.IsApiSession)
        {
            _sessionStorage.Save(profile);
        }

        var window = new WorkspaceWindow(_appState, WorkspaceSection.Overview);
        window.Show();
        Close();
    }
}
