using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.IO;
using WarehouseInventory.Desktop.Models;

namespace WarehouseInventory.Desktop.Services.Api;

public sealed class InventoryApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _downloadCacheDirectory;

    public InventoryApiService(string token)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5267/")
        };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        _downloadCacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MaxPovezet",
            "InventoryFiles");

        Directory.CreateDirectory(_downloadCacheDirectory);
    }

    public async Task<InventoryApiResult<IReadOnlyList<ProductItem>>> GetAllProductsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/inventory-items", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return await FailureFromResponse<IReadOnlyList<ProductItem>>(response, cancellationToken);
            }

            var items = await response.Content.ReadFromJsonAsync<List<InventoryItemDto>>(cancellationToken: cancellationToken);
            if (items is null)
            {
                return InventoryApiResult<IReadOnlyList<ProductItem>>.Failure("API вернул пустой список товаров.");
            }

            var products = new List<ProductItem>(items.Count);
            foreach (var item in items)
            {
                var product = Map(item);
                product.ImagePath = await TryDownloadPreviewAsync(item.Id, cancellationToken);
                products.Add(product);
            }

            return InventoryApiResult<IReadOnlyList<ProductItem>>.Success(products);
        }
        catch (HttpRequestException)
        {
            return InventoryApiResult<IReadOnlyList<ProductItem>>.Failure("Сетевое подключение к API недоступно.");
        }
        catch (TaskCanceledException)
        {
            return InventoryApiResult<IReadOnlyList<ProductItem>>.Failure("Загрузка товаров была прервана.");
        }
    }

    public async Task<InventoryApiResult<ProductItem>> CreateProductAsync(
        ProductItem product,
        string? imagePath,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/inventory-items",
                CreateRequest(product),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await FailureFromResponse<ProductItem>(response, cancellationToken);
            }

            var created = await response.Content.ReadFromJsonAsync<InventoryItemDto>(cancellationToken: cancellationToken);
            if (created is null)
            {
                return InventoryApiResult<ProductItem>.Failure("API не вернул созданный товар.");
            }

            var mapped = Map(created);
            if (!string.IsNullOrWhiteSpace(imagePath))
            {
                var uploadResult = await UploadFileAsync(mapped.Id, imagePath, cancellationToken);
                if (!uploadResult.Succeeded)
                {
                    return InventoryApiResult<ProductItem>.Failure(uploadResult.ErrorMessage, uploadResult.StatusCode);
                }

                mapped.ImagePath = uploadResult.Value;
            }

            return InventoryApiResult<ProductItem>.Success(mapped);
        }
        catch (HttpRequestException)
        {
            return InventoryApiResult<ProductItem>.Failure("Сетевое подключение к API недоступно.");
        }
        catch (TaskCanceledException)
        {
            return InventoryApiResult<ProductItem>.Failure("Создание товара было прервано.");
        }
    }

    public async Task<InventoryApiResult<ProductItem>> UpdateProductAsync(
        ProductItem product,
        string? imagePath,
        bool imageChanged,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/inventory-items/{product.Id}",
                UpdateRequest(product),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await FailureFromResponse<ProductItem>(response, cancellationToken);
            }

            var updated = await response.Content.ReadFromJsonAsync<InventoryItemDto>(cancellationToken: cancellationToken);
            if (updated is null)
            {
                return InventoryApiResult<ProductItem>.Failure("API не вернул обновленный товар.");
            }

            var mapped = Map(updated);
            if (imageChanged && !string.IsNullOrWhiteSpace(imagePath))
            {
                var uploadResult = await UploadFileAsync(mapped.Id, imagePath, cancellationToken);
                if (!uploadResult.Succeeded)
                {
                    return InventoryApiResult<ProductItem>.Failure(uploadResult.ErrorMessage, uploadResult.StatusCode);
                }

                mapped.ImagePath = uploadResult.Value;
            }
            else
            {
                mapped.ImagePath = imagePath;
            }

            return InventoryApiResult<ProductItem>.Success(mapped);
        }
        catch (HttpRequestException)
        {
            return InventoryApiResult<ProductItem>.Failure("Сетевое подключение к API недоступно.");
        }
        catch (TaskCanceledException)
        {
            return InventoryApiResult<ProductItem>.Failure("Обновление товара было прервано.");
        }
    }

    public async Task<InventoryApiResult<bool>> DeleteProductAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/inventory-items/{id}", cancellationToken);
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return InventoryApiResult<bool>.Success(true);
            }

            return await FailureFromResponse<bool>(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return InventoryApiResult<bool>.Failure("Сетевое подключение к API недоступно.");
        }
        catch (TaskCanceledException)
        {
            return InventoryApiResult<bool>.Failure("Удаление товара было прервано.");
        }
    }

    private async Task<InventoryApiResult<string?>> UploadFileAsync(Guid inventoryItemId, string imagePath, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(imagePath);
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(imagePath));
        content.Add(fileContent, "File", Path.GetFileName(imagePath));

        var response = await _httpClient.PostAsync($"api/inventory-items/{inventoryItemId}/files", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return await FailureFromResponse<string?>(response, cancellationToken);
        }

        var fileResponse = await response.Content.ReadFromJsonAsync<InventoryItemFileDto>(cancellationToken: cancellationToken);
        if (fileResponse is null)
        {
            return InventoryApiResult<string?>.Failure("Файл загрузился, но API не вернул его метаданные.");
        }

        var downloadedPreview = await DownloadFileAsync(inventoryItemId, fileResponse.Id, fileResponse.FileName, cancellationToken);
        return downloadedPreview.Succeeded
            ? InventoryApiResult<string?>.Success(downloadedPreview.Value)
            : InventoryApiResult<string?>.Failure(downloadedPreview.ErrorMessage, downloadedPreview.StatusCode);
    }

    private async Task<string?> TryDownloadPreviewAsync(Guid inventoryItemId, CancellationToken cancellationToken)
    {
        var filesResponse = await _httpClient.GetAsync($"api/inventory-items/{inventoryItemId}/files", cancellationToken);
        if (!filesResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var files = await filesResponse.Content.ReadFromJsonAsync<List<InventoryItemFileDto>>(cancellationToken: cancellationToken);
        var firstFile = files?.FirstOrDefault();
        if (firstFile is null)
        {
            return null;
        }

        var downloadResult = await DownloadFileAsync(inventoryItemId, firstFile.Id, firstFile.FileName, cancellationToken);
        return downloadResult.Succeeded ? downloadResult.Value : null;
    }

    private async Task<InventoryApiResult<string?>> DownloadFileAsync(
        Guid inventoryItemId,
        Guid fileId,
        string originalFileName,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"api/inventory-items/{inventoryItemId}/files/{fileId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return await FailureFromResponse<string?>(response, cancellationToken);
        }

        var extension = Path.GetExtension(originalFileName);
        var safeFileName = $"{inventoryItemId}_{fileId}{extension}";
        var filePath = Path.Combine(_downloadCacheDirectory, safeFileName);

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = File.Create(filePath);
        await responseStream.CopyToAsync(fileStream, cancellationToken);

        return InventoryApiResult<string?>.Success(filePath);
    }

    private static ProductItem Map(InventoryItemDto dto)
    {
        return new ProductItem
        {
            Id = dto.Id,
            Name = dto.Name,
            Sku = dto.Sku,
            Category = dto.Category,
            Unit = dto.UnitOfMeasure,
            Quantity = dto.QuantityInStock,
            UnitPrice = dto.UnitPrice,
            WarehouseLocation = dto.WarehouseLocation,
            LastUpdatedUtc = dto.LastUpdatedUtc
        };
    }

    private static CreateInventoryItemRequestDto CreateRequest(ProductItem product) =>
        new()
        {
            Name = product.Name,
            Sku = product.Sku,
            Category = product.Category,
            UnitOfMeasure = product.Unit,
            QuantityInStock = product.Quantity,
            UnitPrice = product.UnitPrice,
            WarehouseLocation = product.WarehouseLocation
        };

    private static UpdateInventoryItemRequestDto UpdateRequest(ProductItem product) =>
        new()
        {
            Name = product.Name,
            Sku = product.Sku,
            Category = product.Category,
            UnitOfMeasure = product.Unit,
            QuantityInStock = product.Quantity,
            UnitPrice = product.UnitPrice,
            WarehouseLocation = product.WarehouseLocation
        };

    private static async Task<InventoryApiResult<T>> FailureFromResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errorMessage = await TryReadErrorAsync(response, cancellationToken);
        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => InventoryApiResult<T>.Failure($"Сессия истекла или токен недействителен: {errorMessage}", (int)response.StatusCode),
            HttpStatusCode.NotFound => InventoryApiResult<T>.Failure($"Объект не найден: {errorMessage}", (int)response.StatusCode),
            HttpStatusCode.Conflict => InventoryApiResult<T>.Failure($"Конфликт данных: {errorMessage}", (int)response.StatusCode),
            HttpStatusCode.BadRequest => InventoryApiResult<T>.Failure($"Некорректный запрос: {errorMessage}", (int)response.StatusCode),
            _ => InventoryApiResult<T>.Failure($"Ошибка API {(int)response.StatusCode}: {errorMessage}", (int)response.StatusCode)
        };
    }

    private static async Task<string> TryReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
            {
                return "без деталей";
            }

            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty("message", out var messageProperty))
            {
                return messageProperty.GetString() ?? "без деталей";
            }

            if (document.RootElement.TryGetProperty("detail", out var detailProperty))
            {
                return detailProperty.GetString() ?? "без деталей";
            }

            return json;
        }
        catch
        {
            return "без деталей";
        }
    }

    private static string GetContentType(string filePath)
    {
        return Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".pdf" => "application/pdf",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "image/png"
        };
    }

    private sealed class InventoryItemDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Sku { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string UnitOfMeasure { get; init; } = string.Empty;
        public int QuantityInStock { get; init; }
        public decimal UnitPrice { get; init; }
        public string WarehouseLocation { get; init; } = string.Empty;
        public DateTime LastUpdatedUtc { get; init; }
    }

    private sealed class CreateInventoryItemRequestDto
    {
        public string Name { get; init; } = string.Empty;
        public string Sku { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string UnitOfMeasure { get; init; } = string.Empty;
        public int QuantityInStock { get; init; }
        public decimal UnitPrice { get; init; }
        public string WarehouseLocation { get; init; } = string.Empty;
    }

    private sealed class UpdateInventoryItemRequestDto
    {
        public string Name { get; init; } = string.Empty;
        public string Sku { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string UnitOfMeasure { get; init; } = string.Empty;
        public int QuantityInStock { get; init; }
        public decimal UnitPrice { get; init; }
        public string WarehouseLocation { get; init; } = string.Empty;
    }

    private sealed class InventoryItemFileDto
    {
        public Guid Id { get; init; }
        public string FileName { get; init; } = string.Empty;
    }
}
