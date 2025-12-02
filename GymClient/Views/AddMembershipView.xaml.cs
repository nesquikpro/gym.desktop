using System.Windows;

namespace GymClient.Views
{
    /// <summary>
    /// Interaction logic for AddMembershipView.xaml
    /// </summary>
    public partial class AddMembershipView : Window
    {
        public AddMembershipView()
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
