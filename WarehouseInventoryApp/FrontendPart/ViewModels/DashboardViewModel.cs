using System.Collections.ObjectModel;
using WarehouseInventory.Desktop.Infrastructure;
using WarehouseInventory.Desktop.Models;

namespace WarehouseInventory.Desktop.ViewModels;

public sealed class DashboardViewModel : ViewModelBase
{
    private readonly Action _logout;
    private ProductItem? _selectedProduct;
    private string _productName = string.Empty;
    private string _sku = string.Empty;
    private string _category = string.Empty;
    private string _unit = "шт";
    private string _quantityInput = "1";
    private string _statusMessage = "Выберите товар из списка или создайте новый. Пока все изменения живут только локально.";
    private string _activeSection = "Товары";
    private string _currentUserName = "Локальный оператор";
    private int _totalProductsInBase = 125;

    public DashboardViewModel(Action logout)
    {
        _logout = logout;
        Products = new ObservableCollection<ProductItem>
        {
            new() { Name = "Сканер штрихкодов Zebra DS2208", Sku = "SCAN-2208", Category = "Электроника", Unit = "шт", Quantity = 14 },
            new() { Name = "Термобумага 58 мм", Sku = "PAPER-58", Category = "Расходники", Unit = "рулон", Quantity = 42 },
            new() { Name = "Маркер перманентный черный", Sku = "OFF-149", Category = "Канцелярия", Unit = "шт", Quantity = 67 },
            new() { Name = "Очиститель контактов", Sku = "CHEM-011", Category = "Химия", Unit = "л", Quantity = 8 }
        };

        ShowOverviewCommand = new RelayCommand(() => ActiveSection = "Обзор");
        ShowProductsCommand = new RelayCommand(() => ActiveSection = "Товары");
        ShowDocumentsCommand = new RelayCommand(() => ActiveSection = "Документы");
        ShowSupplyCommand = new RelayCommand(() => ActiveSection = "Поставки");
        AddProductCommand = new RelayCommand(AddProduct);
        SaveProductCommand = new RelayCommand(UpdateProduct);
        DeleteProductCommand = new RelayCommand(DeleteProduct);
        ClearFormCommand = new RelayCommand(ClearForm);
        LogoutCommand = new RelayCommand(_logout);
    }

    public ObservableCollection<ProductItem> Products { get; }

    public ProductItem? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (!SetProperty(ref _selectedProduct, value))
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            ProductName = value.Name;
            Sku = value.Sku;
            Category = value.Category;
            Unit = value.Unit;
            QuantityInput = value.Quantity.ToString();
            StatusMessage = $"Карточка товара «{value.Name}» загружена в форму для редактирования.";
        }
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

    public string QuantityInput
    {
        get => _quantityInput;
        set => SetProperty(ref _quantityInput, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string ActiveSection
    {
        get => _activeSection;
        private set => SetProperty(ref _activeSection, value);
    }

    public string CurrentUserName
    {
        get => _currentUserName;
        set => SetProperty(ref _currentUserName, value);
    }

    public string TotalProductsText => _totalProductsInBase.ToString();

    public string LastCheckText => "12.04.2024";

    public string ActiveDocumentsText => "2";

    public RelayCommand ShowOverviewCommand { get; }

    public RelayCommand ShowProductsCommand { get; }

    public RelayCommand ShowDocumentsCommand { get; }

    public RelayCommand ShowSupplyCommand { get; }

    public RelayCommand AddProductCommand { get; }

    public RelayCommand SaveProductCommand { get; }

    public RelayCommand DeleteProductCommand { get; }

    public RelayCommand ClearFormCommand { get; }

    public RelayCommand LogoutCommand { get; }

    private void AddProduct()
    {
        if (!ValidateForm(out var quantity))
        {
            return;
        }

        var newProduct = new ProductItem
        {
            Name = ProductName.Trim(),
            Sku = Sku.Trim(),
            Category = Category.Trim(),
            Unit = Unit.Trim(),
            Quantity = quantity
        };

        Products.Add(newProduct);
        SelectedProduct = newProduct;
        _totalProductsInBase++;
        OnPropertyChanged(nameof(TotalProductsText));
        StatusMessage = $"Товар «{newProduct.Name}» добавлен в локальный список.";
    }

    private void UpdateProduct()
    {
        if (SelectedProduct is null)
        {
            StatusMessage = "Сначала выберите товар из списка, а уже потом редактируйте его.";
            return;
        }

        if (!ValidateForm(out var quantity))
        {
            return;
        }

        SelectedProduct.Name = ProductName.Trim();
        SelectedProduct.Sku = Sku.Trim();
        SelectedProduct.Category = Category.Trim();
        SelectedProduct.Unit = Unit.Trim();
        SelectedProduct.Quantity = quantity;

        StatusMessage = $"Карточка товара «{SelectedProduct.Name}» обновлена.";
    }

    private void DeleteProduct()
    {
        if (SelectedProduct is null)
        {
            StatusMessage = "Нечего удалять: товар пока не выбран.";
            return;
        }

        var productName = SelectedProduct.Name;
        Products.Remove(SelectedProduct);
        SelectedProduct = null;
        _totalProductsInBase = Math.Max(0, _totalProductsInBase - 1);
        OnPropertyChanged(nameof(TotalProductsText));
        ClearForm();
        StatusMessage = $"Товар «{productName}» удалён из локального списка.";
    }

    private void ClearForm()
    {
        ProductName = string.Empty;
        Sku = string.Empty;
        Category = string.Empty;
        Unit = "шт";
        QuantityInput = "1";
        SelectedProduct = null;
        StatusMessage = "Форма очищена. Можно заводить новую карточку товара.";
    }

    private bool ValidateForm(out int quantity)
    {
        quantity = 0;

        if (string.IsNullOrWhiteSpace(ProductName) ||
            string.IsNullOrWhiteSpace(Sku) ||
            string.IsNullOrWhiteSpace(Category) ||
            string.IsNullOrWhiteSpace(Unit))
        {
            StatusMessage = "Заполните название, артикул, категорию и единицу измерения.";
            return false;
        }

        if (!int.TryParse(QuantityInput, out quantity) || quantity <= 0)
        {
            StatusMessage = "Количество должно быть положительным целым числом.";
            return false;
        }

        return true;
    }
}
