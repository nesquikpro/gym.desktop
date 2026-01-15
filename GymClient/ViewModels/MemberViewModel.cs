using GymClient.Interfaces;
using GymClient.Models;
using GymClient.Services;
using GymClient.Utils;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GymClient.ViewModels
{
    public class MemberViewModel : BaseViewModel
    {
        private readonly ApiClient _apiClient;
        private readonly IDialogService _dialogService;
        private readonly CsvImportService _csvImportService;

        #region Commands
        public ICommand SaveCommand { get; }
        public ICommand OpenMemberViewCommand { get; }
        public ICommand EditMemberCommand { get; }
        public ICommand DeleteMemberCommand { get; }
        public ICommand ImportCsvCommand { get; }
        #endregion

        #region Collections
        public ObservableCollection<Member> MembersSource { get; } = new();
        private ObservableCollection<Member> _allMembers = new();
        #endregion

        #region Properties
        public bool HasData => MembersSource.Any();
        public bool HasNoData => !HasData;

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set { _isEditMode = value; OnPropertyChanged(nameof(IsEditMode)); OnPropertyChanged(nameof(IsValid)); }
        }

        private Member? _selectedMember;
        public Member? SelectedMember
        {
            get => _selectedMember;
            set
            {
                _selectedMember = value;
                OnPropertyChanged(nameof(SelectedMember));
                if (_selectedMember != null && IsEditMode)
                    LoadSelectedMember();
            }
        }

        private int _id;
        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(nameof(FirstName)); OnPropertyChanged(nameof(IsValid)); }
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(nameof(LastName)); OnPropertyChanged(nameof(IsValid)); }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); OnPropertyChanged(nameof(IsValid)); }
        }

        private string _phoneNumber = string.Empty;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(nameof(PhoneNumber)); OnPropertyChanged(nameof(IsValid)); }
        }

        private DateTime? _dateOfBirth;
        public DateTime? DateOfBirth
        {
            get => _dateOfBirth;
            set { _dateOfBirth = value; OnPropertyChanged(nameof(DateOfBirth)); OnPropertyChanged(nameof(IsValid)); }
        }

        /// <summary>
        /// Проверка валидации всех полей
        /// </summary>
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(FirstName) &&
            !string.IsNullOrWhiteSpace(LastName) &&
            !string.IsNullOrWhiteSpace(Email) &&
            !string.IsNullOrWhiteSpace(PhoneNumber) &&
            DateOfBirth.HasValue;
        #endregion

        #region Events
        /// <summary>
        /// Событие для закрытия окна
        /// </summary>
        public event Action? RequestClose;
        #endregion

        public MemberViewModel(ApiClient apiClient, IDialogService dialogService, CsvImportService csvImportService)
        {
            _apiClient = apiClient;
            _dialogService = dialogService;
            _csvImportService = csvImportService;

            SaveCommand = new RelayCommand(async () =>
            {
                if (!IsValid)
                {
                    await _dialogService.ShowMessage("Заполните все обязательные поля!");
                    return;
                }

                if (IsEditMode)
                    await UpdateMember();
                else
                    await AddMember();
            });

            OpenMemberViewCommand = new RelayCommand(OpenAddMemberView);
            EditMemberCommand = new RelayCommand(async () => await OpenEditMember());
            DeleteMemberCommand = new RelayCommand(async () => await DeleteMember());
            ImportCsvCommand = new RelayCommand(async () =>
            {
                await _csvImportService.ImportMembersFromDialog();
                await LoadMembers();
            });

            MembersSource.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(HasData));
                OnPropertyChanged(nameof(HasNoData));
            };

            _ = LoadMembers();
        }

        #region CRUD

        private async Task AddMember()
        {
            if (!DateOfBirth.HasValue)
            {
                await _dialogService.ShowMessage("Укажите дату рождения!");
                return;
            }

            var member = Member.Create(
                FirstName,
                LastName,
                DateOnly.FromDateTime(DateOfBirth.Value),
                PhoneNumber,
                Email
            );

            var response = await _apiClient.Post(member);

            if (response.IsSuccess && response.Data != null)
            {
                _allMembers.Add(response.Data);
                MembersSource.Add(response.Data);

                RequestClose?.Invoke();
                await _dialogService.ShowMessage("Клиент добавлен!");
            }
            else
            {
                await _dialogService.ShowMessage("Ошибка при добавлении клиента");
            }
        }

        private async Task UpdateMember()
        {
            if (!DateOfBirth.HasValue)
            {
                await _dialogService.ShowMessage("Укажите дату рождения!");
                return;
            }

            var member = Member.Create(
                FirstName,
                LastName,
                DateOnly.FromDateTime(DateOfBirth.Value),
                PhoneNumber,
                Email
            );

            member.Id = Id;

            var response = await _apiClient.Update(member);

            if (response.IsSuccess)
            {
                var existing = _allMembers.FirstOrDefault(m => m.Id == member.Id);
                if (existing != null)
                {
                    int index = _allMembers.IndexOf(existing);
                    _allMembers[index] = member;

                    int idx2 = MembersSource.IndexOf(existing);
                    if (idx2 >= 0)
                        MembersSource[idx2] = member;
                }

                RequestClose?.Invoke();
                await _dialogService.ShowMessage("Данные обновлены!");
            }
            else
            {
                await _dialogService.ShowMessage("Ошибка при обновлении клиента");
            }
        }

        private async Task DeleteMember()
        {
            if (SelectedMember == null) return;

            bool confirm = await _dialogService.ShowConfirmation(
                $"Удалить {SelectedMember.FirstName} {SelectedMember.LastName}?");

            if (!confirm) return;

            var response = await _apiClient.Delete(SelectedMember);

            if (response.IsSuccess)
            {
                _allMembers.Remove(SelectedMember);
                MembersSource.Remove(SelectedMember);
            }
            else
            {
                await _dialogService.ShowMessage("Ошибка при удалении клиента");
            }
        }

        private async Task OpenEditMember()
        {
            if (SelectedMember == null) return;

            Id = SelectedMember.Id;
            FirstName = SelectedMember.FirstName;
            LastName = SelectedMember.LastName;
            Email = SelectedMember.Email;
            PhoneNumber = SelectedMember.PhoneNumber;
            DateOfBirth = SelectedMember.DateOfBirth.ToDateTime(new TimeOnly(0, 0));
            IsEditMode = true;

            var view = new Views.AddMemberView
            {
                DataContext = this,
                Owner = System.Windows.Application.Current.MainWindow
            };

            RequestClose += () => view.Close();
            view.ShowDialog();

            OnPropertyChanged(nameof(MembersSource));
        }

        private void OpenAddMemberView()
        {
            Id = 0;
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            DateOfBirth = null;
            IsEditMode = false;

            var view = new Views.AddMemberView
            {
                DataContext = this,
                Owner = System.Windows.Application.Current.MainWindow
            };

            RequestClose += () => view.Close();
            view.ShowDialog();
        }
        public async void InitializeAsync() 
        { 
            await LoadMembers(); 
        }
        private void LoadSelectedMember()
        {
            if (SelectedMember == null) return;

            Id = SelectedMember.Id;
            FirstName = SelectedMember.FirstName;
            LastName = SelectedMember.LastName;
            Email = SelectedMember.Email;
            PhoneNumber = SelectedMember.PhoneNumber;
            DateOfBirth = SelectedMember.DateOfBirth.ToDateTime(new TimeOnly(0, 0));
        }

        #endregion

        #region Helpers

        private async Task LoadMembers()
        {
            var result = await _apiClient.GetAll<Member>("api/members");

            if (result.Data == null)
            {
                await _dialogService.ShowMessage("Не удалось загрузить список клиентов");
                return;
            }

            _allMembers = new ObservableCollection<Member>(result.Data);

            MembersSource.Clear();
            foreach (var m in _allMembers)
                MembersSource.Add(m);
        }

        #endregion
    }
}
