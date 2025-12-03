using GymClient.ViewModels;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.DependencyInjection;

namespace GymClient.Views
{
    /// <summary>
    /// Interaction logic for VisitView.xaml
    /// </summary>
    public partial class VisitView : System.Windows.Controls.UserControl
    {
        public VisitView()
        {
            InitializeComponent();       
            this.DataContext = App.Services.GetRequiredService<VisitViewModel>();
        }

        private void CartesianChart_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is VisitViewModel vm)
            {
                var columnSeries = vm.ChartSeries[0] as ColumnSeries<int>;
                if (columnSeries != null)
                {
                    columnSeries.Values = Array.Empty<int>();
                }
                vm.ChartXAxes[0].Labels = Array.Empty<string>();
            }

            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            SelectedPeriodComboBox.SelectedIndex = 0;
        }

        private void DatePicker_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = true;
        }
    }
}
