using GymClient.Interfaces;
using GymClient.Models;
using GymClient.Services;
using GymClient.Utils;
using GymClient.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GymClient.ViewModels
{
    public class VisitViewModel : BaseViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly ApiClient _apiClient;
        private readonly IExportService _visitExportService;
        public ISeries[] ChartSeries { get; private set; }
        public Axis[] ChartXAxes { get; private set; }
        public Axis[] ChartYAxes { get; private set; }
        public SolidColorPaint LegendTextPaint { get; private set; }
        public bool CanSave => SelectedMember != null;
        public bool HasData => FilteredVisits != null && FilteredVisits.Count > 0;
        public bool HasNoData => !HasData;
        private Chip? _selectedMember;
        public Chip? SelectedMember
        {
            get => _selectedMember;
            set
            {
                _selectedMember = value;
                OnPropertyChanged(nameof(SelectedMember));
                OnPropertyChanged(nameof(CanSave));
            }
        }
        private VisitDto? _selectedVisit;
        public VisitDto? SelectedVisit
        {
            get => _selectedVisit;
            set
            {
                _selectedVisit = value;
                OnPropertyChanged(nameof(SelectedVisit));
            }
        }
        public ObservableCollection<Chip> ActiveMembers { get; set; } = new();
        public ObservableCollection<VisitDto> Visits { get; set; } = new();
        private ObservableCollection<VisitDto> _filteredVisits = new();
        public ObservableCollection<VisitDto> FilteredVisits
        {
            get => _filteredVisits;
            set
            {
                _filteredVisits = value;
                OnPropertyChanged(nameof(FilteredVisits));
            }
        }
        private string _selectedPeriodFilter = "Все";
        public string SelectedPeriodFilter
        {
            get => _selectedPeriodFilter;
            set
            {
                if (_selectedPeriodFilter == value) return;
                _selectedPeriodFilter = value;
                OnPropertyChanged(nameof(SelectedPeriodFilter));
                ApplyFilters();
            }
        }
        private string? _searchText;
        public string? SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                ApplyFilters();
            }
        }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ICommand ApplyFiltersCommand => new RelayCommand(ApplyFilters);
        public ICommand ExportToPdfCommand => new RelayCommand(ExportToPdf);
        public ICommand ExportToExcelCommand => new RelayCommand(ExportToExcel);
        public ICommand OpenVisitViewCommand => new RelayCommand(OpenVisitView);
        public ICommand AddVisitCommand => new RelayCommand(async () => await AddVisit());
        public ICommand DeleteVisitCommand => new RelayCommand(async () => await DeleteVisit());

        public VisitViewModel(ApiClient apiClient, IDialogService dialogService, IExportService visitExportService)
        {
            _apiClient = apiClient;
            _dialogService = dialogService;
            _visitExportService = visitExportService;

            Visits.CollectionChanged += (s, e) => ApplyFilters();
            InitializeAsync();
            InitializeChart();
        }

        private void InitializeChart()
        {
            var colorPaint = new SolidColorPaint(SKColor.Parse("03A9F4"));

            ChartSeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = new ObservableCollection<int>(),
                    Name = "Посещения"
                }
            };

            ChartXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = new List<string>(),
                    LabelsPaint = colorPaint
                }
            };

            ChartYAxes = new Axis[]
            {
                new Axis
                {
                    MinStep = 1,
                    Labeler = value => value.ToString("0"),
                    LabelsPaint = colorPaint
                }
            };

            LegendTextPaint = colorPaint;
        }

        public async void InitializeAsync()
        {
            await LoadVisits();
        }

        public int GetTodayVisits()
        {
            var filtered = Visits.AsEnumerable();
            var today = DateTime.Today;
            filtered = filtered.Where(v => v.VisitDateTime.Date == today);
            return filtered.Count();
        }

        public void ApplyFilters()
        {
            var filtered = Visits.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(v =>
                    v.MemberFullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    v.ChipNumber.Contains(SearchText));
            }

            if (StartDate.HasValue)
                filtered = filtered.Where(v => v.VisitDateTime.Date >= StartDate.Value.Date);
            if (EndDate.HasValue)
                filtered = filtered.Where(v => v.VisitDateTime.Date <= EndDate.Value.Date);

            switch (SelectedPeriodFilter)
            {
                case "Сегодня":
                    var today = DateTime.Today;
                    filtered = filtered.Where(v => v.VisitDateTime.Date == today);
                    break;
                case "Месяц":
                    var firstDayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    filtered = filtered.Where(v => v.VisitDateTime.Date >= firstDayOfMonth);
                    break;
                case "Год":
                    var firstDayOfYear = new DateTime(DateTime.Today.Year, 1, 1);
                    filtered = filtered.Where(v => v.VisitDateTime.Date >= firstDayOfYear);
                    break;
            }

            FilteredVisits = new ObservableCollection<VisitDto>(filtered);
            OnPropertyChanged(nameof(HasData));
            OnPropertyChanged(nameof(HasNoData));
            UpdateChart();
        }

        private void UpdateChart()
        {
            if (ChartSeries == null || ChartSeries.Length == 0) return;

            var columnSeries = ChartSeries[0] as ColumnSeries<int>;
            var xAxis = ChartXAxes[0];

            if (FilteredVisits.Count == 0)
            {
                columnSeries.Values = Array.Empty<int>();
                xAxis.Labels = Array.Empty<string>();
                return;
            }

            var grouped = FilteredVisits
                .GroupBy(v => v.VisitDateTime.Date)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key.ToString("dd.MM.yyyy"), g => g.Count());

            columnSeries.Values = grouped.Values.ToArray();
            xAxis.Labels = grouped.Keys.ToArray();
        }

        private async Task LoadVisits()
        {
            var visitResult = await _apiClient.GetAll<VisitDto>("/api/visits/full");
            var membersResult = await _apiClient.GetAll<Chip>("/api/chips/active_with_name");

            if (visitResult.IsSuccess && visitResult.Data != null)
            {
                Visits.Clear();
                foreach (var visit in visitResult.Data) Visits.Add(visit);
            }

            if (membersResult.IsSuccess && membersResult.Data != null)
            {
                ActiveMembers.Clear();
                foreach (var member in membersResult.Data) ActiveMembers.Add(member);
            }
        }

        private void OpenVisitView()
        {
            var addVisitView = new AddVisitView
            {
                DataContext = this,
            };
            addVisitView.ShowDialog();
        }

        private async Task DeleteVisit()
        {
            if (SelectedVisit == null) return;

            bool confirm = await _dialogService.ShowConfirmation(
                $"Удалить визит клиента: {SelectedVisit.MemberFullName} ?");

            if (!confirm) return;

            var visitId = new Visit { Id = SelectedVisit.Id };
            var response = await _apiClient.Delete(visitId);

            if (response.IsSuccess) await LoadVisits();
            ApplyFilters();
        }

        private async Task AddVisit()
        {
            if (SelectedMember == null)
            {
                await _dialogService.ShowMessage("Выберите клиента!");
                return;
            }

            var chipId = ActiveMembers.FirstOrDefault(c => c.Id == SelectedMember.Id);

            if (chipId == null) return;

            var newVisit = new Visit { ChipId = chipId.Id };
            var response = await _apiClient.Post(newVisit);

            var selectedMemberName = SelectedMember.MemberFullName;

            if (response.IsSuccess && response.Data != null)
            {
                await LoadVisits();
                ApplyFilters();
                await _dialogService.ShowMessage($"Здравствуйте, {selectedMemberName}!");
            }
        }

        private void ExportToPdf()
        {
            if (FilteredVisits == null || FilteredVisits.Count == 0)
            {
                _dialogService.ShowMessage("Нет данных для экспорта.");
                return;
            }

            _visitExportService.ExportToPdf(FilteredVisits);
        }

        private void ExportToExcel()
        {
            if (FilteredVisits == null || FilteredVisits.Count == 0)
            {
                _dialogService.ShowMessage("Нет данных для экспорта.");
                return;
            }

            _visitExportService.ExportToExcel(FilteredVisits);
        }
    }
}
