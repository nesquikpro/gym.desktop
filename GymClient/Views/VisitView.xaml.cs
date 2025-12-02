using GymClient.ViewModels;
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
    }
}
