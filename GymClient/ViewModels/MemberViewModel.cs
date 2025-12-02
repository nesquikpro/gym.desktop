using GymClient.Interfaces;
using GymClient.Models;
using GymClient.Services;
using GymClient.Utils;
using GymClient.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GymClient.ViewModels
{
    public class MemberViewModel : BaseViewModel
    {
        private static readonly Random _random = new();
        public ICommand AddMemberCommand { get; }
        public ICommand OpenMemberViewCommand { get; }
        public ICommand EditMemberCommand { get; }
        public ICommand DeleteMemberCommand { get; }
        public ICommand ImportCsvCommand { get; }
        public readonly IDialogService _dialogService;
        private readonly ApiClient _apiClient;
        private readonly CsvImportService _csvImportService;
        private Member? _selectedMember;
        public Member? SelectedMember
        {
            get => _selectedMember;
            set
            {
                _selectedMember = value;
                OnPropertyChanged(nameof(SelectedMember));
            }
        }
        private string _firstName = "test";
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged(nameof(FirstName));
            }
        }
        private string _lastName = "test";
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged(nameof(LastName));
            }
        }
        private DateOnly _dateOfBirth = new(1990, 12, 1);
        public DateOnly DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                _dateOfBirth = value;
                OnPropertyChanged(nameof(DateOfBirth));
            }
        }
        private string _email = "g@gmail.com";
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }
        private string _phoneNumber = "1" + string.Concat(Enumerable.Range(0, 10).Select(_ => _random.Next(0, 10)));
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;
                OnPropertyChanged(nameof(PhoneNumber));
            }
        }
        private int _id = 0;
        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

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
        public ObservableCollection<Member> MembersSource { get; set; } = new();
        public ObservableCollection<Member> FilteredMembers { get; set; } = new();
        private ObservableCollection<Member> _allMembers = new();

        public MemberViewModel(ApiClient apiClient, IDialogService dialogService, CsvImportService csvImportService)
        {
            _apiClient = apiClient;
            _dialogService = dialogService;
            _csvImportService = csvImportService;

            OpenMemberViewCommand = new RelayCommand(OpenAddMemberView);
            AddMemberCommand = new RelayCommand(async () => await AddMember());

            EditMemberCommand = new RelayCommand(async () => await OpenEditMember());

            DeleteMemberCommand = new RelayCommand(async () => await DeleteMember());

            ImportCsvCommand = new RelayCommand(async () =>
            {
                await _csvImportService.ImportMembersFromDialog();
                InitializeAsync();
            });

            InitializeAsync();
        }

        public async void InitializeAsync()
        {
            await LoadMembers();
        }

        private void ApplyFilters()
        {
            List<Member> filtered;
            if (string.IsNullOrWhiteSpace(SearchText))
                filtered = _allMembers.ToList();
            else
            {
                string lower = SearchText.ToLower();
                filtered = _allMembers.Where(m =>
                    (m.FirstName != null && m.FirstName.ToLower().Contains(lower)) ||
                    (m.LastName != null && m.LastName.ToLower().Contains(lower)) ||
                    (m.Email != null && m.Email.ToLower().Contains(lower)) ||
                    (m.PhoneNumber != null && m.PhoneNumber.ToLower().Contains(lower))
                ).ToList();
            }

            MembersSource.Clear();
            foreach (var m in filtered)
                MembersSource.Add(m);
        }

        private async Task DeleteMember()
        {
            if (SelectedMember == null) return;

            bool confirm = await _dialogService.ShowConfirmation(
                $"Удалить {SelectedMember.FirstName} {SelectedMember.LastName} клиента?");
            if (!confirm) return;

            var response = await _apiClient.Delete(SelectedMember);

            if (response.IsSuccess)
            {
                _allMembers.Remove(SelectedMember);
                ApplyFilters();
            }
            else
            {
                await _dialogService.ShowMessage($"Ошибка!");
            }
        }

        private async Task OpenEditMember()
        {
            IsEditMode = true;

            if (SelectedMember == null) return;

            var editableMember = SelectedMember.Clone();

            var editView = new AddMemberView
            {
                DataContext = editableMember,
                Owner = System.Windows.Application.Current.MainWindow
            };

            bool? result = editView.ShowDialog();

            if (result == true)
            {
                await _apiClient.Update((Member)editableMember);
                await _dialogService.ShowMessage($"Данные обновлены!");
                InitializeAsync();
            }
        }

        private void OpenAddMemberView()
        {
            IsEditMode = false;

            var addMemberWindow = new AddMemberView
            {
                DataContext = this,
                Owner = System.Windows.Application.Current.MainWindow
            };
            addMemberWindow.ShowDialog();
        }

        private async Task LoadMembers()
        {
            var result = await _apiClient.GetAll<Member>("api/members");
            if (result.Data == null)
            {
                await _dialogService.ShowMessage($"Не удалось загрузить данные: {result.ErrorMessage}");
                return;
            }

            _allMembers = new ObservableCollection<Member>(result.Data);
            ApplyFilters();
        }

        private async Task AddMember()
        {
            var newMember = Member.Create(
                FirstName,
                LastName,
                DateOfBirth,
                PhoneNumber,
                Email
            );

            var response = await _apiClient.Post(newMember);
            if (response.IsSuccess && response.Data != null)
            {
                _allMembers.Add(response.Data);
                await _dialogService.ShowMessage("Клиент добавлен!");
                ApplyFilters();
            }
            else
            {
                await _dialogService.ShowMessage($"Ошибка!");
            }
        }
    }
}
