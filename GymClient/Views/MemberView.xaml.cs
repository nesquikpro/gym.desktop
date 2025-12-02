using GymClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace GymClient.Views
{
    /// <summary>
    /// Interaction logic for MemberView.xaml
    /// </summary>
    public partial class MemberView : System.Windows.Controls.UserControl
    {
        public MemberView()
        {
            InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<MemberViewModel>();
        }
    }
}
