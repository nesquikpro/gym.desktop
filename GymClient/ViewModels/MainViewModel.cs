using GymClient.Utils;
using System.Windows.Input;
using System.Windows.Threading;

namespace GymClient.ViewModels
{
    public class MainViewModel: BaseViewModel
    {
        public MemberViewModel MembersVM { get; }
        public MembershipViewModel MembershipsVM { get; }
        public ChipViewModel ChipsVM { get; }
        public VisitViewModel VisitsVM { get; }
        public ICommand LogoutCommand { get; }
        public event Action? LogoutRequested;
        private readonly DispatcherTimer _timer;

        private DateTime _currentTime;
        public DateTime CurrentTime
        {
            get => _currentTime;
            set 
            { 
                _currentTime = value; 
                OnPropertyChanged(); 
            }
        }
        private string? _currentUsername;
        public string? CurrentUsername
        {
            get => _currentUsername;
            set
            {
                if (_currentUsername != value)
                {
                    _currentUsername = value;
                    OnPropertyChanged(nameof(CurrentUsername));
                }
            }
        }
        private System.Windows.Controls.UserControl _currentView;
        public System.Windows.Controls.UserControl CurrentView
        {
            get => _currentView;
            set 
            { 
                _currentView = value; 
                OnPropertyChanged(); 
            }
        }

        private string _recordsSummary;
        public string RecordsSummary
        {
            get => _recordsSummary;
            set 
            {
                _recordsSummary = value; 
                OnPropertyChanged(); 
            }
        }
        public ICommand OpenMembersCommand { get; }
        public ICommand OpenMembershipsCommand { get; }
        public ICommand OpenChipsCommand { get; }
        public ICommand OpenVisitsCommand { get; }
        public MainViewModel(MemberViewModel membersVM, MembershipViewModel membershipsVM, ChipViewModel chipsVM, VisitViewModel visitsVM)
        {
            MembersVM = membersVM;
            MembershipsVM = membershipsVM;
            ChipsVM = chipsVM;
            VisitsVM = visitsVM;

            MembersVM.MembersSource.CollectionChanged += (_, __) => UpdateRecordsSummary();
            MembershipsVM.MembershipSource.CollectionChanged += (_, __) => UpdateRecordsSummary();
            ChipsVM.ActiveChips.CollectionChanged += (_, __) => UpdateRecordsSummary();
            VisitsVM.Visits.CollectionChanged += (_, __) => UpdateRecordsSummary();

            UpdateRecordsSummary();

            InitializeStatusBar();
           
            LogoutCommand = new RelayCommand(() => Logout());
            OpenMembersCommand = new RelayCommand(() => MembersVM.InitializeAsync());
            OpenMembershipsCommand = new RelayCommand(() => MembershipsVM.InitializeAsync());
            OpenChipsCommand = new RelayCommand(() => ChipsVM.InitializeAsync());
            OpenVisitsCommand = new RelayCommand(() => VisitsVM.InitializeAsync());

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _timer.Tick += (s, e) => CurrentTime = DateTime.Now;
            _timer.Start();
        }

        private void UpdateRecordsSummary()
        {
            RecordsSummary = $"Клиенты: {MembersVM.MembersSource.Count} | " +
                             $"Абонементы: {MembershipsVM.MembershipSource.Count} | " +
                             $"Активные чипы: {ChipsVM.ActiveChips.Count} | " +
                             $"Визиты за сегодня: {VisitsVM.GetTodayVisits()}";
        }

        public void Logout()
        {
            CurrentUsername = null;
            LogoutRequested?.Invoke();
        }

        private void InitializeStatusBar()
        {
            CurrentTime = DateTime.Now;
        }
    }
}
