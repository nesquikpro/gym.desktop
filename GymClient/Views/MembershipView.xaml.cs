using GymClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace GymClient.Views
{
    /// <summary>
    /// Interaction logic for MembershipView.xaml
    /// </summary>
    public partial class MembershipView : System.Windows.Controls.UserControl
    {
        public MembershipView()
        {
            InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<MembershipViewModel>();
        }
    }
}
