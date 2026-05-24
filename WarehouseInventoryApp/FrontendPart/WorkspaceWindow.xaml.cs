using System.Windows;
using WarehouseInventory.Desktop.Models;
using WarehouseInventory.Desktop.Services.Api;
using WarehouseInventory.Desktop.ViewModels;

namespace WarehouseInventory.Desktop;

public partial class WorkspaceWindow : Window
{
    private readonly AuthSessionStorage _sessionStorage = new();
    private readonly InventoryAppState _state;

    public WorkspaceWindow(InventoryAppState state, WorkspaceSection section)
    {
        _state = state;
        InitializeComponent();
        DataContext = new WorkspaceViewModel(state, section, Logout);
    }

    private void Logout()
    {
        if (_state.IsApiSession)
        {
            _sessionStorage.Clear();
        }

        var authWindow = new MainWindow();
        authWindow.Show();
        Close();
    }
}
