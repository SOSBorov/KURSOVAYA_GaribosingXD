using System.Globalization;
using System.IO;
using Microsoft.Win32;
using WarehouseInventory.Desktop.Infrastructure;
using WarehouseInventory.Desktop.Models;
using WarehouseInventory.Desktop.Services.Api;

namespace WarehouseInventory.Desktop.ViewModels;

public sealed class ProductsSectionViewModel : ViewModelBase
{
    private readonly InventoryAppState _state;
    private readonly InventoryApiService? _inventoryApiService;
    private ProductItem? _selectedProduct;
    private string _productName = string.Empty;
    private string _sku = string.Empty;
    private string _category = string.Empty;
    private string _unit = "шт";
    private string _quantityInput = "1";
    private string _unitPriceInput = "0";
    private string _warehouseLocation = string.Empty;
    private string? _imagePath;
    private string _statusMessage = "Подключите API и нажмите на товар из списка или создайте новую карточку.";
    private bool _isBusy;
    private bool _imageChanged;

    public ProductsSectionViewModel(InventoryAppState state)
    {
        _state = state;

        if (!string.IsNullOrWhiteSpace(_state.AuthToken))
        {
            _inventoryApiService = new InventoryApiService(_state.AuthToken);
            _statusMessage = "Загружаю товары из API...";
        }
        else
        {
            _statusMessage = "API-сессия не найдена. Список товаров работает только после входа через сервер.";
        }

        LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync, () => !IsBusy && _inventoryApiService is not null);
        AddProductCommand = new AsyncRelayCommand(AddProductAsync, () => !IsBusy && _inventoryApiService is not null);
        SaveProductCommand = new AsyncRelayCommand(UpdateProductAsync, () => !IsBusy && _inventoryApiService is not null && SelectedProduct is not null);
        DeleteProductCommand = new AsyncRelayCommand(DeleteProductAsync, () => !IsBusy && _inventoryApiService is not null && CanDeleteProducts && SelectedProduct is not null);
        ClearFormCommand = new RelayCommand(ClearForm, () => !IsBusy);
        ChooseImageCommand = new RelayCommand(ChooseImage, () => !IsBusy);

        if (_inventoryApiService is not null)
        {
            _ = LoadProductsAsync();
        }
    }

    public string Header => "Товары";

    public string Description => "Складской каталог с загрузкой из API, обновлением карточек и привязкой фото товара.";

    public IEnumerable<ProductItem> Products => _state.Products;

    public bool CanDeleteProducts => _state.IsAdmin;

    public string AccessBadge => _state.IsApiSession
        ? "API-сессия"
        : _state.IsAdmin ? "Администратор" : "Оператор";

    public string DeleteAccessText => _state.IsAdmin
        ? "Удаление товара доступно для администратора и отправляет запрос на сервер."
        : "Удаление товара заблокировано: нужен уровень доступа Администратор.";

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
                SaveProductCommand.RaiseCanExecuteChanged();
                DeleteProductCommand.RaiseCanExecuteChanged();
                return;
            }

            ProductName = value.Name;
            Sku = value.Sku;
            Category = value.Category;
            Unit = value.Unit;
            QuantityInput = value.Quantity.ToString(CultureInfo.InvariantCulture);
            UnitPriceInput = value.UnitPrice.ToString("0.##", CultureInfo.InvariantCulture);
            WarehouseLocation = value.WarehouseLocation;
            ImagePath = value.ImagePath;
            _imageChanged = false;
            StatusMessage = $"Карточка товара «{value.Name}» загружена из API.";
            SaveProductCommand.RaiseCanExecuteChanged();
            DeleteProductCommand.RaiseCanExecuteChanged();
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

    public string UnitPriceInput
    {
        get => _unitPriceInput;
        set => SetProperty(ref _unitPriceInput, value);
    }

    public string WarehouseLocation
    {
        get => _warehouseLocation;
        set => SetProperty(ref _warehouseLocation, value);
    }

    public string? ImagePath
    {
        get => _imagePath;
        set
        {
            if (!SetProperty(ref _imagePath, value))
            {
                return;
            }

            OnPropertyChanged(nameof(HasImage));
            OnPropertyChanged(nameof(ImageLabel));
        }
    }

    public bool HasImage => !string.IsNullOrWhiteSpace(ImagePath);

    public string ImageLabel => HasImage ? "Фото товара подключено" : "Фото пока не выбрано";

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

            LoadProductsCommand.RaiseCanExecuteChanged();
            AddProductCommand.RaiseCanExecuteChanged();
            SaveProductCommand.RaiseCanExecuteChanged();
            DeleteProductCommand.RaiseCanExecuteChanged();
            ClearFormCommand.RaiseCanExecuteChanged();
            ChooseImageCommand.RaiseCanExecuteChanged();
        }
    }

    public AsyncRelayCommand LoadProductsCommand { get; }

    public AsyncRelayCommand AddProductCommand { get; }

    public AsyncRelayCommand SaveProductCommand { get; }

    public AsyncRelayCommand DeleteProductCommand { get; }

    public RelayCommand ClearFormCommand { get; }

    public RelayCommand ChooseImageCommand { get; }

    private async Task LoadProductsAsync()
    {
        if (_inventoryApiService is null)
        {
            StatusMessage = "API-сервис для товаров недоступен.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Загружаю товары из API...";

            var result = await _inventoryApiService.GetAllProductsAsync(CancellationToken.None);
            if (!result.Succeeded || result.Value is null)
            {
                StatusMessage = BuildApiErrorMessage(result.ErrorMessage, result.StatusCode, "Не удалось загрузить товары.");
                return;
            }

            _state.Products.Clear();
            foreach (var product in result.Value)
            {
                _state.Products.Add(product);
            }

            _state.TotalProductsInBase = _state.Products.Count;
            _state.LastCheckDate = DateTime.Today.ToString("dd.MM.yyyy");
            OnPropertyChanged(nameof(Products));
            StatusMessage = $"Товары загружены: {_state.Products.Count} поз.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddProductAsync()
    {
        if (_inventoryApiService is null)
        {
            StatusMessage = "API-сервис для товаров недоступен.";
            return;
        }

        if (!ValidateForm(out var quantity, out var unitPrice))
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Создаю товар на сервере...";

            var draft = new ProductItem
            {
                Name = ProductName.Trim(),
                Sku = Sku.Trim(),
                Category = Category.Trim(),
                Unit = Unit.Trim(),
                Quantity = quantity,
                UnitPrice = unitPrice,
                WarehouseLocation = WarehouseLocation.Trim()
            };

            var result = await _inventoryApiService.CreateProductAsync(draft, ImagePath, CancellationToken.None);
            if (!result.Succeeded || result.Value is null)
            {
                StatusMessage = BuildApiErrorMessage(result.ErrorMessage, result.StatusCode, "Не удалось создать товар.");
                return;
            }

            _state.Products.Add(result.Value);
            _state.TotalProductsInBase = _state.Products.Count;
            _state.LastCheckDate = DateTime.Today.ToString("dd.MM.yyyy");
            OnPropertyChanged(nameof(Products));
            SelectedProduct = result.Value;
            StatusMessage = $"Товар «{result.Value.Name}» создан и отправлен в API.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UpdateProductAsync()
    {
        if (_inventoryApiService is null)
        {
            StatusMessage = "API-сервис для товаров недоступен.";
            return;
        }

        if (SelectedProduct is null)
        {
            StatusMessage = "Сначала выберите товар из списка.";
            return;
        }

        if (!ValidateForm(out var quantity, out var unitPrice))
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Обновляю карточку товара...";

            var draft = new ProductItem
            {
                Id = SelectedProduct.Id,
                Name = ProductName.Trim(),
                Sku = Sku.Trim(),
                Category = Category.Trim(),
                Unit = Unit.Trim(),
                Quantity = quantity,
                UnitPrice = unitPrice,
                WarehouseLocation = WarehouseLocation.Trim(),
                LastUpdatedUtc = SelectedProduct.LastUpdatedUtc,
                ImagePath = ImagePath
            };

            var result = await _inventoryApiService.UpdateProductAsync(draft, ImagePath, _imageChanged, CancellationToken.None);
            if (!result.Succeeded || result.Value is null)
            {
                StatusMessage = BuildApiErrorMessage(result.ErrorMessage, result.StatusCode, "Не удалось обновить товар.");
                return;
            }

            CopyProductValues(SelectedProduct, result.Value);
            _state.LastCheckDate = DateTime.Today.ToString("dd.MM.yyyy");
            _imageChanged = false;
            StatusMessage = $"Карточка товара «{SelectedProduct.Name}» обновлена через API.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteProductAsync()
    {
        if (_inventoryApiService is null)
        {
            StatusMessage = "API-сервис для товаров недоступен.";
            return;
        }

        if (!CanDeleteProducts)
        {
            StatusMessage = "Удалять товары может только администратор.";
            return;
        }

        if (SelectedProduct is null)
        {
            StatusMessage = "Нечего удалять: товар не выбран.";
            return;
        }

        var product = SelectedProduct;

        try
        {
            IsBusy = true;
            StatusMessage = $"Удаляю товар «{product.Name}»...";

            var result = await _inventoryApiService.DeleteProductAsync(product.Id, CancellationToken.None);
            if (!result.Succeeded)
            {
                StatusMessage = BuildApiErrorMessage(result.ErrorMessage, result.StatusCode, "Не удалось удалить товар.");
                return;
            }

            _state.Products.Remove(product);
            _state.TotalProductsInBase = _state.Products.Count;
            _state.LastCheckDate = DateTime.Today.ToString("dd.MM.yyyy");
            OnPropertyChanged(nameof(Products));
            ClearForm();
            StatusMessage = $"Товар «{product.Name}» удалён с сервера.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearForm()
    {
        ProductName = string.Empty;
        Sku = string.Empty;
        Category = string.Empty;
        Unit = "шт";
        QuantityInput = "1";
        UnitPriceInput = "0";
        WarehouseLocation = string.Empty;
        ImagePath = null;
        _imageChanged = false;
        _selectedProduct = null;
        OnPropertyChanged(nameof(SelectedProduct));
        SaveProductCommand.RaiseCanExecuteChanged();
        DeleteProductCommand.RaiseCanExecuteChanged();
        StatusMessage = "Форма очищена. Можно создать новую карточку товара.";
    }

    private void ChooseImage()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Выберите фото товара",
            Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.webp"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        ImagePath = dialog.FileName;
        _imageChanged = true;
        StatusMessage = $"Фото товара выбрано: {Path.GetFileName(dialog.FileName)}.";
    }

    private bool ValidateForm(out int quantity, out decimal unitPrice)
    {
        quantity = 0;
        unitPrice = 0m;

        if (string.IsNullOrWhiteSpace(ProductName) ||
            string.IsNullOrWhiteSpace(Sku) ||
            string.IsNullOrWhiteSpace(Category) ||
            string.IsNullOrWhiteSpace(Unit) ||
            string.IsNullOrWhiteSpace(WarehouseLocation))
        {
            StatusMessage = "Заполните название, SKU, категорию, единицу измерения и локацию склада.";
            return false;
        }

        if (!int.TryParse(QuantityInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out quantity) || quantity < 0)
        {
            StatusMessage = "Количество должно быть целым числом от 0 и выше.";
            return false;
        }

        var normalizedPrice = UnitPriceInput.Replace(',', '.');
        if (!decimal.TryParse(normalizedPrice, NumberStyles.Number, CultureInfo.InvariantCulture, out unitPrice) || unitPrice < 0)
        {
            StatusMessage = "Цена должна быть числом от 0 и выше.";
            return false;
        }

        return true;
    }

    private static string BuildApiErrorMessage(string? errorMessage, int? statusCode, string fallback)
    {
        if (statusCode == 401)
        {
            return "API вернул 401. Возможно, токен устарел и нужно войти заново.";
        }

        return string.IsNullOrWhiteSpace(errorMessage)
            ? fallback
            : $"{fallback} {errorMessage}";
    }

    private static void CopyProductValues(ProductItem target, ProductItem source)
    {
        target.Id = source.Id;
        target.Name = source.Name;
        target.Sku = source.Sku;
        target.Category = source.Category;
        target.Unit = source.Unit;
        target.Quantity = source.Quantity;
        target.UnitPrice = source.UnitPrice;
        target.WarehouseLocation = source.WarehouseLocation;
        target.LastUpdatedUtc = source.LastUpdatedUtc;
        target.ImagePath = source.ImagePath;
    }
}
