using System.IO;
using Microsoft.Win32;
using WarehouseInventory.Desktop.Infrastructure;
using WarehouseInventory.Desktop.Models;

namespace WarehouseInventory.Desktop.ViewModels;

public sealed class ProfileSectionViewModel : ViewModelBase
{
    private readonly InventoryAppState _state;
    private bool _isEditing;
    private string _employeeFullName;
    private string _shift;
    private string _warehouseZone;
    private string _notes;
    private string _statusMessage;
    private string? _profileImagePath;

    public ProfileSectionViewModel(InventoryAppState state)
    {
        _state = state;
        _employeeFullName = _state.EmployeeFullName;
        _shift = _state.Shift;
        _warehouseZone = _state.WarehouseZone;
        _notes = _state.Notes;
        _profileImagePath = _state.ProfileImagePath;
        _statusMessage = "Профиль загружен. Можно просмотреть рабочие данные сотрудника.";
        ToggleEditProfileCommand = new RelayCommand(ToggleEditProfile);
        ChooseAvatarCommand = new RelayCommand(ChooseAvatar);
    }

    public string Header => "Профиль сотрудника";

    public string Description => "Карточка сотрудника склада: ФИО, зона ответственности, смена, уровень доступа и рабочие заметки.";

    public string CurrentUserName => _state.CurrentUserName;

    public string EmployeeFullName
    {
        get => _employeeFullName;
        set => SetProperty(ref _employeeFullName, value);
    }

    public string Role => _state.OperatorRole;

    public string Shift
    {
        get => _shift;
        set => SetProperty(ref _shift, value);
    }

    public string WarehouseZone
    {
        get => _warehouseZone;
        set => SetProperty(ref _warehouseZone, value);
    }

    public string AccessLevel => _state.AccessLevel;

    public string EmployeeId => _state.EmployeeId;

    public string LastLogin => _state.LastLogin;

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public string? ProfileImagePath
    {
        get => _profileImagePath;
        set
        {
            if (!SetProperty(ref _profileImagePath, value))
            {
                return;
            }

            OnPropertyChanged(nameof(HasProfileImage));
            OnPropertyChanged(nameof(AvatarStatusText));
        }
    }

    public string TasksCompletedToday => _state.TasksCompletedToday.ToString();

    public string InventoryAccuracyPercent => $"{_state.InventoryAccuracyPercent}%";

    public string TotalProducts => _state.TotalProductsInBase.ToString();

    public bool HasProfileImage => !string.IsNullOrWhiteSpace(ProfileImagePath);

    public string AvatarStatusText => HasProfileImage ? "Аватар загружен" : "Аватар пока не выбран";

    public bool IsEditing
    {
        get => _isEditing;
        private set
        {
            if (!SetProperty(ref _isEditing, value))
            {
                return;
            }

            OnPropertyChanged(nameof(ToggleEditButtonText));
        }
    }

    public string ToggleEditButtonText => IsEditing ? "Сохранить профиль" : "Редактировать профиль";

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public RelayCommand ToggleEditProfileCommand { get; }

    public RelayCommand ChooseAvatarCommand { get; }

    private void ToggleEditProfile()
    {
        if (!IsEditing)
        {
            IsEditing = true;
            StatusMessage = "Режим редактирования включен. Можно изменить ФИО, смену, зону и заметки.";
            return;
        }

        _state.EmployeeFullName = EmployeeFullName.Trim();
        _state.Shift = Shift.Trim();
        _state.WarehouseZone = WarehouseZone.Trim();
        _state.Notes = Notes.Trim();
        _state.ProfileImagePath = ProfileImagePath;

        IsEditing = false;
        StatusMessage = "Профиль сохранен локально.";
    }

    private void ChooseAvatar()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Выберите аватар сотрудника",
            Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.webp"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        ProfileImagePath = dialog.FileName;
        _state.ProfileImagePath = dialog.FileName;
        StatusMessage = $"Аватар выбран: {Path.GetFileName(dialog.FileName)}.";
    }
}
