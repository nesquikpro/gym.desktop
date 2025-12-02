using GymClient.Interfaces;
using GymClient.Models;
using GymClient.Services;
using GymClient.Utils;
using GymClient.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;

namespace GymClient.ViewModels
{
    public class MembershipViewModel : BaseViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly IQrCodeService _qrCodeService;
        private readonly ApiClient _apiClient;
        public bool HasData => MembershipSource != null && MembershipSource.Count > 0;
        public bool HasNoData => !HasData;
        public ObservableCollection<Member>? Members { get; set; }
        public ObservableCollection<Membership> MembershipSource { get; set; } = new();
        private ObservableCollection<Membership> _allMemberships = new();
        public bool CanSave => SelectedMember != null;
        private bool _isCardPayment;
        public bool IsCardPayment
        {
            get => _isCardPayment;
            set
            {
                _isCardPayment = value;
                OnPropertyChanged(nameof(IsCardPayment));
            }
        }

        private Member? _selectedMember;
        public Member? SelectedMember
        {
            get => _selectedMember;
            set
            {
                _selectedMember = value;
                OnPropertyChanged(nameof(SelectedMember));
                OnPropertyChanged(nameof(CanSave));
            }
        }
        private Membership? _selectedMembership;
        public Membership? SelectedMembership
        {
            get => _selectedMembership;
            set
            {
                _selectedMembership = value;
                OnPropertyChanged(nameof(SelectedMembership));
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
        private const int DailyRate = 200;
        private int _paymentAmount = 2000;
        public int PaymentAmount
        {
            get => _paymentAmount;
            private set
            {
                _paymentAmount = value;
                OnPropertyChanged(nameof(PaymentAmount));
            }
        }
        public ICommand AddMembershipCommand { get; }
        public ICommand OpenAddMemberViewCommand { get; }
        public ICommand OpenFreezeDialogCommand { get; }
        public ICommand DeleteMembershipCommand { get; }
        public ICommand ApplyFiltersCommand { get; }
        public ICommand UnfreezeCommand { get; }

        private DateOnly _startDate = DateOnly.FromDateTime(DateTime.Today);
        public DateTime StartDateTime
        {
            get => _startDate.ToDateTime(TimeOnly.MinValue);
            set
            {
                _startDate = DateOnly.FromDateTime(value);
                OnPropertyChanged(nameof(StartDateTime));

                if (!_endDate.HasValue)
                {
                    EndDateTime = _startDate.AddMonths(1).ToDateTime(TimeOnly.MinValue);
                }

                RecalculatePayment();
            }
        }

        private DateOnly? _endDate;
        public DateTime? EndDateTime
        {
            get => _endDate.HasValue ? _endDate.Value.ToDateTime(TimeOnly.MinValue) : null;
            set
            {
                if (value.HasValue)
                    _endDate = DateOnly.FromDateTime(value.Value);
                else
                    _endDate = null;

                OnPropertyChanged(nameof(EndDateTime));
                RecalculatePayment();
            }
        }

        public MembershipViewModel(ApiClient apiClient, IDialogService dialogService, IQrCodeService qrCodeService)
        {
            _apiClient = apiClient;
            _dialogService = dialogService;
            _qrCodeService = qrCodeService;

            OpenAddMemberViewCommand = new RelayCommand(OpenAddMembershipView);
            AddMembershipCommand = new RelayCommand(async () => await AddMembership());
            DeleteMembershipCommand = new RelayCommand(async () => await DeleteMembership());
            ApplyFiltersCommand = new RelayCommand(() => ApplyFilters());
            OpenFreezeDialogCommand = new RelayCommand(async () => await OpenFreezeDialog());
            UnfreezeCommand = new RelayCommand(async () => await UnfreezeMembership());
            InitializeAsync();
        }

        private async Task OpenFreezeDialog()
        {
            if (SelectedMembership == null)
            {
                await _dialogService.ShowMessage("Выберите абонемент!");
                return;
            }

            var freezeCopy = (Membership)SelectedMembership.Clone();

            freezeCopy.FreezeStartDate = DateOnly.FromDateTime(DateTime.Today);

            var freezeWindow = new FreezeMembershipView
            {
                DataContext = freezeCopy,
                Owner = System.Windows.Application.Current.MainWindow
            };

            if (freezeWindow.ShowDialog() == true)
            {
                await FreezeMembership(freezeCopy);
            }
        }

        private async Task FreezeMembership(Membership freezeData)
        {
            if (!freezeData.FreezeStartDate.HasValue || !freezeData.FreezeEndDate.HasValue)
            {
                await _dialogService.ShowMessage("Укажите даты заморозки!");
                return;
            }

            if (freezeData.FreezeStartDate < DateOnly.FromDateTime(DateTime.Today))
            {
                await _dialogService.ShowMessage("Дата начала заморозки не может быть в прошлом!");
                return;
            }

            if (freezeData.FreezeEndDate < freezeData.FreezeStartDate)
            {
                await _dialogService.ShowMessage("Дата конца должна быть позже начала!");
                return;
            }

            SelectedMembership.IsFrozen = true;
            SelectedMembership.FreezeStartDate = freezeData.FreezeStartDate;
            SelectedMembership.FreezeEndDate = freezeData.FreezeEndDate;

            var response = await _apiClient.Update(SelectedMembership);
            if (response.IsSuccess)
            {
                await _dialogService.ShowMessage("Абонемент заморожен!");
                await LoadMembership();
            }
            else
            {
                await _dialogService.ShowMessage("Ошибка!");
            }
        }

        private async Task UnfreezeMembership()
        {
            if (SelectedMembership == null)
            {
                await _dialogService.ShowMessage("Выберите абонемент!");
                return;
            }

            SelectedMembership.IsFrozen = false;
            SelectedMembership.FreezeStartDate = null;
            SelectedMembership.FreezeEndDate = null;

            var response = await _apiClient.Update(SelectedMembership);

            if (response.IsSuccess)
            {
                await _dialogService.ShowMessage("Абонемент разморожен!");
                await LoadMembership();
            }
            else
            {
                await _dialogService.ShowMessage("Ошибка разморозки!");
            }
        }

        public async void InitializeAsync()
        {
            await LoadMembership();
        }

        private async Task SendPaymentEmail()
        {
            decimal amount = _paymentAmount;
            string paymentUrl = $"https://yourserver.com/pay?memberId={SelectedMember.Id}&amount={amount}";

            byte[] qrPng = _qrCodeService.GeneratePng(paymentUrl);

            var request = new
            {
                ToEmail = SelectedMember.Email,
                MemberName = SelectedMember.FullName,
                Amount = PaymentAmount,
                PaymentUrl = paymentUrl,
                QrPngBase64 = Convert.ToBase64String(qrPng)
            };

            var response = await _apiClient.PostPaymentEmailAsync("api/paymentemail", request);

            if (response.IsSuccess)
            {
                var emailBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#03A9F4")!);
                await _dialogService.ShowMessageWithColoredEmail(
                    "Ссылка для оплаты отправлена на электронную почту: ",
                    SelectedMember.Email,
                    emailBrush);
            }
            else
            {
                await _dialogService.ShowMessage($"Ошибка!");
            }
        }

        private void OpenAddMembershipView()
        {
            _endDate = _startDate.AddMonths(1);
            var addMembershipWindow = new AddMembershipView
            {
                DataContext = this,
                Owner = System.Windows.Application.Current.MainWindow
            };
            addMembershipWindow.ShowDialog();
        }

        private void RecalculatePayment()
        {
            if (!_endDate.HasValue)
            {
                PaymentAmount = 0;
                return;
            }

            int days = (_endDate.Value.DayNumber - _startDate.DayNumber) + 1;
            if (days < 1) days = 1;
            PaymentAmount = days * DailyRate;
        }

        private async Task LoadMembership()
        {
            var membersResult = await _apiClient.GetAll<Member>("/api/members");

            if (membersResult.Data == null) return;

            Members = new ObservableCollection<Member>(membersResult.Data);

            var membershipsResult = await _apiClient.GetAll<Membership>("/api/memberships");
            if (!membershipsResult.IsSuccess || membershipsResult.Data == null)
            {
                await _dialogService.ShowMessage($"Не удалось загрузить данные: {membershipsResult.ErrorMessage}");
                return;
            }

            _allMemberships = new ObservableCollection<Membership>(membershipsResult.Data);

            foreach (var membership in _allMemberships)
            {
                var member = membersResult.Data.FirstOrDefault(m => m.Id == membership.MemberId);
                membership.MemberFullName = member != null ? member.FullName : "Не найден";

                if (membership.IsFrozen && membership.FreezeEndDate.HasValue && membership.FreezeEndDate.Value < DateOnly.FromDateTime(DateTime.Today))
                {
                    membership.IsFrozen = false;
                    membership.FreezeStartDate = null;
                    membership.FreezeEndDate = null;
                    await _apiClient.Update(membership);
                }
            }

            ApplyFilters();
        }

        private async Task AddMembership()
        {
            if (SelectedMember == null)
            {
                await _dialogService.ShowMessage("Выберите клиента!");
                return;
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var hasActiveMembership = _allMemberships.Any(m =>
                m.MemberId == SelectedMember.Id &&
                !m.IsFrozen &&
                m.StartDate <= today &&
                m.EndDate >= today
            );

            if (hasActiveMembership)
            {
                await _dialogService.ShowMessage("У данного пользователя  уже есть активный абонемент!");
                return;
            }

            var newMembership = new Membership
            {
                MemberId = SelectedMember.Id,
                StartDate = _startDate,
                EndDate = _endDate,
                IsPaidByCard = IsCardPayment,
                PaymentQRCode = IsCardPayment ? $"pay://membership/{Guid.NewGuid()}" : null
            };

            var response = await _apiClient.Post(newMembership);

            if (response.IsSuccess)
            {
                if (IsCardPayment)
                {
                    await SendPaymentEmail();
                }
                else
                {
                    await _dialogService.ShowMessage("Абонемент добавлен!");
                }
                await LoadMembership();
            }
            else
            {
                await _dialogService.ShowMessage($"Ошибка!");
            }
        }

        private async Task DeleteMembership()
        {
            if (SelectedMembership == null) return;

            bool confirm = await _dialogService.ShowConfirmation("Удалить выбранный абонемент?");
            if (!confirm) return;

            var response = await _apiClient.Delete(SelectedMembership);
            if (response.IsSuccess)
            {
                _allMemberships.Remove(SelectedMembership);
                await LoadMembership();
            }
            else
            {
                await _dialogService.ShowMessage($"Ошибка!");
            }
        }

        private void ApplyFilters()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                MembershipSource = new ObservableCollection<Membership>(_allMemberships);
            }
            else
            {
                var lower = SearchText.ToLower();

                MembershipSource = new ObservableCollection<Membership>(
                    _allMemberships.Where(m =>
                    {
                        string name = m.MemberFullName?.ToLower() ?? "";

                        string start = m.StartDate.ToString().ToLower();
                        string end = m.EndDate.ToString().ToLower();

                        return name.Contains(lower)
                               || start.Contains(lower)
                               || end.Contains(lower);
                    })
                );
            }

            OnPropertyChanged(nameof(MembershipSource));
        }
    }
}