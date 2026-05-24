using WarehouseInventory.Desktop.ViewModels;

namespace WarehouseInventory.Desktop.Models;

public enum InventoryCheckStatus
{
    Match,
    Shortage,
    Surplus
}

public sealed class InventoryCheckItem : ViewModelBase
{
    private Guid _productId;
    private string _productName = string.Empty;
    private string _sku = string.Empty;
    private int _expectedQuantity;
    private int _actualQuantity;
    private InventoryCheckStatus _status = InventoryCheckStatus.Match;

    public Guid ProductId
    {
        get => _productId;
        set => SetProperty(ref _productId, value);
    }

    public string ProductName
    {
        get => _productName;
        set => SetProperty(ref _productName, value);
    }

    public string Sku
    {
        get => _sku;
        set => SetProperty(ref _sku, value);
    }

    public int ExpectedQuantity
    {
        get => _expectedQuantity;
        set => SetProperty(ref _expectedQuantity, value);
    }

    public int ActualQuantity
    {
        get => _actualQuantity;
        set => SetProperty(ref _actualQuantity, value);
    }

    public InventoryCheckStatus Status
    {
        get => _status;
        set
        {
            if (!SetProperty(ref _status, value))
            {
                return;
            }

            OnPropertyChanged(nameof(StatusText));
        }
    }

    public string StatusText => Status switch
    {
        InventoryCheckStatus.Match => "Совпадает",
        InventoryCheckStatus.Shortage => "Недостача",
        InventoryCheckStatus.Surplus => "Избыток",
        _ => string.Empty
    };
}
