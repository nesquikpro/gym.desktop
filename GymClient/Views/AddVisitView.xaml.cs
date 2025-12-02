using System.Windows;

namespace GymClient.Views
{
    /// <summary>
    /// Interaction logic for AddVisitView.xaml
    /// </summary>
    public partial class AddVisitView : Window
    {
        public AddVisitView()
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
