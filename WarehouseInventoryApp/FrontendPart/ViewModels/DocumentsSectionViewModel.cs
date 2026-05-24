using System.IO;
using Microsoft.Win32;
using WarehouseInventory.Desktop.Infrastructure;
using WarehouseInventory.Desktop.Models;

namespace WarehouseInventory.Desktop.ViewModels;

public sealed class DocumentsSectionViewModel : ViewModelBase
{
    private readonly InventoryAppState _state;
    private string _statusMessage = "Отчет можно скачать в текстовый файл.";

    public DocumentsSectionViewModel(InventoryAppState state)
    {
        _state = state;
        DownloadReportCommand = new RelayCommand(DownloadReport);
    }

    public string Header => "Отчеты";

    public string Description => "Короткий отчет по инвентаризации: сколько проверено, где совпадает и где есть расхождения.";

    public int CheckedCount => _state.InventoryChecks.Count;

    public int MatchesCount => _state.InventoryChecks.Count(x => x.Status == InventoryCheckStatus.Match);

    public int ShortagesCount => _state.InventoryChecks.Count(x => x.Status == InventoryCheckStatus.Shortage);

    public int SurplusCount => _state.InventoryChecks.Count(x => x.Status == InventoryCheckStatus.Surplus);

    public int MismatchesCount => ShortagesCount + SurplusCount;

    public string LastSavedText => _state.InventorySavedAtUtc is null
        ? "Пока не сохранено"
        : _state.InventorySavedAtUtc.Value.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public RelayCommand DownloadReportCommand { get; }

    private void DownloadReport()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Скачать отчет",
            Filter = "Текстовый отчет|*.txt",
            FileName = $"inventory-report-{DateTime.Now:yyyyMMdd-HHmm}.txt"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var lines = new List<string>
        {
            "ОТЧЕТ ПО ИНВЕНТАРИЗАЦИИ",
            $"Дата выгрузки: {DateTime.Now:dd.MM.yyyy HH:mm}",
            $"Последнее сохранение: {LastSavedText}",
            string.Empty,
            $"Проверено позиций: {CheckedCount}",
            $"Совпадает: {MatchesCount}",
            $"Недостача: {ShortagesCount}",
            $"Избыток: {SurplusCount}",
            $"Несоответствий: {MismatchesCount}",
            string.Empty,
            "ДЕТАЛИ:"
        };

        foreach (var check in _state.InventoryChecks)
        {
            lines.Add($"{check.ProductName} | {check.Sku} | ожидалось: {check.ExpectedQuantity} | факт: {check.ActualQuantity} | статус: {check.StatusText}");
        }

        File.WriteAllLines(dialog.FileName, lines);
        StatusMessage = $"Отчет сохранен: {Path.GetFileName(dialog.FileName)}";
    }
}
