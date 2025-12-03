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

        private void DatePicker_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
