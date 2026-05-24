using System.Collections.ObjectModel;
using WarehouseInventory.Desktop.Infrastructure;
using WarehouseInventory.Desktop.Models;

namespace WarehouseInventory.Desktop.ViewModels;

public sealed class InventorySectionViewModel : ViewModelBase
{
    private readonly InventoryAppState _state;
    private ProductItem? _selectedProduct;
    private string _scanSku = string.Empty;
    private string _actualQuantityInput = "0";
    private string _statusMessage = "Откройте экран, отсканируйте товар или выберите его из списка, затем введите фактическое количество.";

    public InventorySectionViewModel(InventoryAppState state)
    {
        _state = state;
        FindBySkuCommand = new RelayCommand(FindBySku);
        SavePositionCommand = new RelayCommand(SavePosition, () => SelectedProduct is not null);
        SaveInventoryCommand = new RelayCommand(SaveInventory, () => Checks.Count > 0);
    }

    public string Header => "Остатки";

    public string Description => "Инвентаризация: сканирование или выбор товара, ввод факта, сравнение с ожидаемым количеством и сохранение результата.";

    public ObservableCollection<ProductItem> Products => _state.Products;

    public ObservableCollection<InventoryCheckItem> Checks => _state.InventoryChecks;

    public ProductItem? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (!SetProperty(ref _selectedProduct, value))
            {
                return;
            }

            if (value is not null)
            {
                ScanSku = value.Sku;
                ActualQuantityInput = value.Quantity.ToString();
            }

            SavePositionCommand.RaiseCanExecuteChanged();
        }
    }

    public string ScanSku
    {
        get => _scanSku;
        set => SetProperty(ref _scanSku, value);
    }

    public string ActualQuantityInput
    {
        get => _actualQuantityInput;
        set => SetProperty(ref _actualQuantityInput, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public RelayCommand FindBySkuCommand { get; }

    public RelayCommand SavePositionCommand { get; }

    public RelayCommand SaveInventoryCommand { get; }

    private void FindBySku()
    {
        if (string.IsNullOrWhiteSpace(ScanSku))
        {
            StatusMessage = "Введите SKU или выберите товар из списка.";
            return;
        }

        var product = _state.Products.FirstOrDefault(x =>
            string.Equals(x.Sku, ScanSku.Trim(), StringComparison.OrdinalIgnoreCase));

        if (product is null)
        {
            StatusMessage = $"Товар с SKU \"{ScanSku}\" не найден.";
            return;
        }

        SelectedProduct = product;
        StatusMessage = $"Позиция \"{product.Name}\" найдена. Укажите фактическое количество.";
    }

    private void SavePosition()
    {
        if (SelectedProduct is null)
        {
            StatusMessage = "Сначала выберите товар.";
            return;
        }

        if (!int.TryParse(ActualQuantityInput, out var actual) || actual < 0)
        {
            StatusMessage = "Фактическое количество должно быть целым числом от 0.";
            return;
        }

        var status = actual == SelectedProduct.Quantity
            ? InventoryCheckStatus.Match
            : actual < SelectedProduct.Quantity
                ? InventoryCheckStatus.Shortage
                : InventoryCheckStatus.Surplus;

        var existing = _state.InventoryChecks.FirstOrDefault(x => x.ProductId == SelectedProduct.Id);
        if (existing is null)
        {
            _state.InventoryChecks.Add(new InventoryCheckItem
            {
                ProductId = SelectedProduct.Id,
                ProductName = SelectedProduct.Name,
                Sku = SelectedProduct.Sku,
                ExpectedQuantity = SelectedProduct.Quantity,
                ActualQuantity = actual,
                Status = status
            });
        }
        else
        {
            existing.ProductName = SelectedProduct.Name;
            existing.Sku = SelectedProduct.Sku;
            existing.ExpectedQuantity = SelectedProduct.Quantity;
            existing.ActualQuantity = actual;
            existing.Status = status;
        }

        SaveInventoryCommand.RaiseCanExecuteChanged();
        StatusMessage = $"Позиция \"{SelectedProduct.Name}\" проверена: {existingStatusText(status)}.";
    }

    private void SaveInventory()
    {
        _state.InventorySavedAtUtc = DateTime.UtcNow;
        _state.LastCheckDate = DateTime.Now.ToString("dd.MM.yyyy");
        StatusMessage = $"Инвентаризация сохранена. Проверено позиций: {_state.InventoryChecks.Count}.";
    }

    private static string existingStatusText(InventoryCheckStatus status) => status switch
    {
        InventoryCheckStatus.Match => "✅ совпадает",
        InventoryCheckStatus.Shortage => "⚠️ недостача",
        InventoryCheckStatus.Surplus => "🔺 избыток",
        _ => string.Empty
    };
}
