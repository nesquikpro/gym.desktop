using System.Windows;

namespace GymClient.Views
{
    /// <summary>
    /// Interaction logic for FreezeMembershipView.xaml
    /// </summary>
    public partial class FreezeMembershipView : Window
    {
        public FreezeMembershipView()
        {
            InitializeComponent();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
