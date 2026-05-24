using WarehouseInventory.Desktop.Infrastructure;
using WarehouseInventory.Desktop.Models;

namespace WarehouseInventory.Desktop.ViewModels;

public sealed class WorkspaceViewModel : ViewModelBase
{
    private readonly Action _logout;
    private ViewModelBase _currentSectionView;
    private readonly InventoryAppState _state;
    private WorkspaceSection _currentSection;

    public WorkspaceViewModel(
        InventoryAppState state,
        WorkspaceSection section,
        Action logout)
    {
        _state = state;
        _logout = logout;

        if (section == WorkspaceSection.Inventory && _state.IsAdmin)
        {
            section = WorkspaceSection.Overview;
        }

        _currentSection = section;
        _currentSectionView = CreateSectionViewModel(section);

        OpenOverviewCommand = new RelayCommand(() => NavigateTo(WorkspaceSection.Overview));
        OpenProductsCommand = new RelayCommand(() => NavigateTo(WorkspaceSection.Products));
        OpenReportsCommand = new RelayCommand(() => NavigateTo(WorkspaceSection.Reports));
        OpenInventoryCommand = new RelayCommand(
            () => NavigateTo(WorkspaceSection.Inventory),
            () => IsOperator);
        OpenProfileCommand = new RelayCommand(() => NavigateTo(WorkspaceSection.Profile));
        LogoutCommand = new RelayCommand(_logout);
    }

    public WorkspaceSection CurrentSection
    {
        get => _currentSection;
        private set
        {
            if (!SetProperty(ref _currentSection, value))
            {
                return;
            }

            OnPropertyChanged(nameof(IsOverviewSection));
            OnPropertyChanged(nameof(IsProductsSection));
            OnPropertyChanged(nameof(IsReportsSection));
            OnPropertyChanged(nameof(IsInventorySection));
            OnPropertyChanged(nameof(IsProfileSection));
        }
    }

    public bool IsOverviewSection => CurrentSection == WorkspaceSection.Overview;

    public bool IsProductsSection => CurrentSection == WorkspaceSection.Products;

    public bool IsReportsSection => CurrentSection == WorkspaceSection.Reports;

    public bool IsInventorySection => CurrentSection == WorkspaceSection.Inventory;

    public bool IsProfileSection => CurrentSection == WorkspaceSection.Profile;

    public bool IsOperator => !_state.IsAdmin;

    public string CurrentUserName => _state.CurrentUserName;

    public string SessionModeDescription => _state.SessionModeDescription;

    public ViewModelBase CurrentSectionView
    {
        get => _currentSectionView;
        private set => SetProperty(ref _currentSectionView, value);
    }

    public RelayCommand OpenOverviewCommand { get; }

    public RelayCommand OpenProductsCommand { get; }

    public RelayCommand OpenReportsCommand { get; }

    public RelayCommand OpenInventoryCommand { get; }

    public RelayCommand OpenProfileCommand { get; }

    public RelayCommand LogoutCommand { get; }

    private void NavigateTo(WorkspaceSection section)
    {
        if (CurrentSection == section)
        {
            return;
        }

        CurrentSection = section;
        CurrentSectionView = CreateSectionViewModel(section);
    }

    private ViewModelBase CreateSectionViewModel(WorkspaceSection section)
    {
        return section switch
        {
            WorkspaceSection.Overview => new OverviewSectionViewModel(_state),
            WorkspaceSection.Products => new ProductsSectionViewModel(_state),
            WorkspaceSection.Reports => new DocumentsSectionViewModel(_state),
            WorkspaceSection.Inventory => new InventorySectionViewModel(_state),
            WorkspaceSection.Profile => new ProfileSectionViewModel(_state),
            _ => new OverviewSectionViewModel(_state)
        };
    }
}
