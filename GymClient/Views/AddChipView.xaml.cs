using System.Windows;

namespace GymClient.Views
{
    /// <summary>
    /// Interaction logic for AddChipView.xaml
    /// </summary>
    public partial class AddChipView : Window
    {
        public AddChipView()
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
