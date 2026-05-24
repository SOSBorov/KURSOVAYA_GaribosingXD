using WarehouseInventory.Desktop.Models;

namespace WarehouseInventory.Desktop.ViewModels;

public sealed class OverviewSectionViewModel : ViewModelBase
{
    private readonly InventoryAppState _state;

    public OverviewSectionViewModel(InventoryAppState state)
    {
        _state = state;
    }

    public string Header => "Обзор склада";

    public string Description => "Быстрая сводка по товарам, последней инвентаризации и активной работе в клиентском приложении.";

    public string TotalProductsText => _state.TotalProductsInBase.ToString();

    public string LastCheckText => _state.LastCheckDate;

    public string ActiveDocumentsText => _state.ActiveDocuments.ToString();

    public string UserText => _state.CurrentUserName;
}
