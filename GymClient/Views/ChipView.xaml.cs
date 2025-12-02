using GymClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace GymClient.Views
{
    /// <summary>
    /// Interaction logic for ChipView.xaml
    /// </summary>
    public partial class ChipView : System.Windows.Controls.UserControl
    {
        public ChipView()
        {
            InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ChipViewModel>();
        }
    }
}
