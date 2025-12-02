using GymClient.Interfaces;
using GymClient.Models;
using GymClient.Services;
using GymClient.Utils;
using GymClient.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GymClient.ViewModels
{
    public class ChipViewModel : BaseViewModel
    {
        public readonly IDialogService _dialogService;
        private readonly ApiClient _apiClient;
        public ICommand AddChipCommand { get; }
        public ICommand OpenAddChipViewCommand { get; }
        public ICommand DeleteChipCommand { get; }
        public ICommand OpenEditChipCommand { get; }
        public bool CanSave => SelectedMember != null || SelectedChip != null;
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    ApplyFilters();
                }
            }
        }

        private string _editableMember;
        public string EditableMember
        {
            get => _editableMember;
            set
            {
                _editableMember = value;
                OnPropertyChanged(nameof(EditableMember));
            }
        }

        private MemberDto? _selectedMember;
        public MemberDto? SelectedMember
        {
            get => _selectedMember;
            set
            {
                _selectedMember = value;
                OnPropertyChanged(nameof(SelectedMember));
                OnPropertyChanged(nameof(CanSave));
            }
        }

        private Chip? _selectedChip;
        public Chip? SelectedChip
        {
            get => _selectedChip;
            set
            {
                _selectedChip = value;
                OnPropertyChanged(nameof(SelectedChip));
                OnPropertyChanged(nameof(CanSave));
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                _isEditMode = value;
                OnPropertyChanged(nameof(IsEditMode));
            }
        }

        private bool _isActiveMembersVisible;
        public bool IsActiveMembersVisible
        {
            get => _isActiveMembersVisible;
            set
            {
                _isActiveMembersVisible = value;
                OnPropertyChanged(nameof(IsActiveMembersVisible));
            }
        }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }

        public ObservableCollection<MemberDto> Members { get; set; } = new ObservableCollection<MemberDto>();
        public ObservableCollection<Chip> InactiveChips { get; set; } = new();
        public ObservableCollection<Chip> ActiveChips { get; set; } = new();
        private List<Chip> _allActiveChips = new();
        private List<Chip> _allInactiveChips = new();
        private List<MemberDto> _allMembers = new();

        public ChipViewModel(ApiClient apiClient, IDialogService dialogService)
        {
            _apiClient = apiClient;
            _dialogService = dialogService;

            OpenAddChipViewCommand = new RelayCommand(OpenChipView);
            AddChipCommand = new RelayCommand(async () => await AddChip());     
            DeleteChipCommand = new RelayCommand(async () => await DeleteChip());
            OpenEditChipCommand = new RelayCommand(async () => await OpenEditChipView());

            InitializeAsync();
        }

        public async void InitializeAsync()
        {
            await LoadChips();
        }

        private void OpenChipView()
        {
            IsEditMode = false;
            IsActive = false;
            IsActiveMembersVisible = true;

            var addChipView = new AddChipView
            {
                DataContext = this,
                Owner = System.Windows.Application.Current.MainWindow
            };
            addChipView.ShowDialog();
        }

        private async Task OpenEditChipView()
        {
            if (SelectedChip == null) return;
            IsEditMode = true;
            IsActiveMembersVisible = false;
            EditableMember = SelectedChip.MemberFullName;
            IsActive = SelectedChip.IsActive;

            var editChipView = new AddChipView
            {
                DataContext = this,
                Owner = System.Windows.Application.Current.MainWindow
            };

            if (editChipView.ShowDialog() == true)
            {
                Chip chip = (Chip)SelectedChip.Clone();
                chip.IsActive = IsActive;
                await EditChip(chip);
            }
        }

        private async Task EditChip(Chip editableChip)
        {
            var response = await _apiClient.Update(editableChip);
            if (response.IsSuccess)
            {
                await _dialogService.ShowMessage("Данные обновлены!");
                await LoadChips();
            }
            else
            {
                await _dialogService.ShowMessage($"Ошибка!");
            }
        }

        private async Task DeleteChip()
        {
            if (SelectedChip == null) return;

            bool confirm = await _dialogService.ShowConfirmation(
                $"Удалить чип: {SelectedChip.ChipNumber} клиента: {SelectedChip.MemberFullName}?");
            if (!confirm) return;

            var response = await _apiClient.Delete(SelectedChip);

            if (response.IsSuccess)
            {
                await LoadChips();
            }
            else
            {
                await _dialogService.ShowMessage($"Ошибка!");
            }
        }

        private async Task AddChip()
        {
            if (SelectedMember == null)
            {
                await _dialogService.ShowMessage("Выберите клиента!");
                return;
            }

            var newChip = new Chip
            {
                MemberId = SelectedMember.Id,
                IsActive = IsActive
            };

            var response = await _apiClient.Post(newChip);
            if (response.IsSuccess && response.Data != null)
            {
                await _dialogService.ShowMessage("Чип добавлен!");
                await LoadChips();
            }
        }

        private async Task LoadChips()
        {
            var activeChips = await _apiClient.GetAll<Chip>("api/chips/active");
            var inactiveChips = await _apiClient.GetAll<Chip>("api/chips/inactive");
            var membersResult = await _apiClient.GetAll<MemberDto>("api/members/available_for_chip");

            if (activeChips.Data == null || inactiveChips.Data == null || membersResult.Data == null)
            {
                await _dialogService.ShowMessage($"Не удалось загрузить данные!");
                return;
            }

            RefreshDataGrids(activeChips.Data, inactiveChips.Data, membersResult.Data);
        }

        private void RefreshObservableCollection<T>(ObservableCollection<T> target, List<T> source)
        {
            target.Clear();
            foreach (var item in source)
                target.Add(item);
        }

        private void ApplyFilters()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                RefreshObservableCollection(ActiveChips, _allActiveChips);
                RefreshObservableCollection(InactiveChips, _allInactiveChips);
            }
            else
            {
                var lower = SearchText.ToLower();

                var filteredActiveChips = _allActiveChips.Where(a =>
                    (!string.IsNullOrEmpty(a.MemberFullName) && a.MemberFullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(a.ChipNumber) && a.ChipNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                ).ToList();

                var filteredInactiveChips = _allInactiveChips.Where(a =>
                    (!string.IsNullOrEmpty(a.MemberFullName) && a.MemberFullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(a.ChipNumber) && a.ChipNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                ).ToList();

                RefreshObservableCollection(ActiveChips, filteredActiveChips);
                RefreshObservableCollection(InactiveChips, filteredInactiveChips);
            }
        }

        private void RefreshDataGrids(List<Chip> activeChips, List<Chip> inactiveChips, List<MemberDto> membersResult)
        {
            _allMembers.Clear();
            _allActiveChips.Clear();
            _allInactiveChips.Clear();

            foreach (var member in membersResult)
            {
                _allMembers.Add(member);
            }

            _allActiveChips = activeChips.ToList();

            _allInactiveChips = inactiveChips.ToList();

            RefreshObservableCollection(ActiveChips, _allActiveChips);
            RefreshObservableCollection(InactiveChips, _allInactiveChips);
            RefreshObservableCollection(Members, _allMembers);

            OnPropertyChanged(nameof(ActiveChips));
            OnPropertyChanged(nameof(InactiveChips));
        }
    }
}
