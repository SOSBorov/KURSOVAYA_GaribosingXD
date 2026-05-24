using WarehouseInventory.Desktop.ViewModels;

namespace WarehouseInventory.Desktop.Models;

public sealed class ProductItem : ViewModelBase
{
    private Guid _id;
    private string _name = string.Empty;
    private string _sku = string.Empty;
    private string _category = string.Empty;
    private string _unit = string.Empty;
    private int _quantity;
    private decimal _unitPrice;
    private string _warehouseLocation = string.Empty;
    private DateTime _lastUpdatedUtc;
    private string? _imagePath;

    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Sku
    {
        get => _sku;
        set => SetProperty(ref _sku, value);
    }

    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value);
    }

    public string Unit
    {
        get => _unit;
        set => SetProperty(ref _unit, value);
    }

    public int Quantity
    {
        get => _quantity;
        set => SetProperty(ref _quantity, value);
    }

    public decimal UnitPrice
    {
        get => _unitPrice;
        set => SetProperty(ref _unitPrice, value);
    }

    public string WarehouseLocation
    {
        get => _warehouseLocation;
        set => SetProperty(ref _warehouseLocation, value);
    }

    public DateTime LastUpdatedUtc
    {
        get => _lastUpdatedUtc;
        set => SetProperty(ref _lastUpdatedUtc, value);
    }

    public string? ImagePath
    {
        get => _imagePath;
        set => SetProperty(ref _imagePath, value);
    }
}
