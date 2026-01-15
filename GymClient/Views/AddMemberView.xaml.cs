using System.Windows;
using System.Windows.Controls;
namespace GymClient.Views
{
    /// <summary>
    /// Interaction logic for AddMemberView.xaml
    /// </summary>
    public partial class AddMemberView : Window
    {
        public AddMemberView()
        {
            InitializeComponent();
            Loaded += (_, __) => ValidateAll();
        }
        private void ValidateAll()
        {
            FirstNameBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty)?.UpdateSource();
            LastNameBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty)?.UpdateSource();
            EmailBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty)?.UpdateSource();
            PhoneBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty)?.UpdateSource();
            DateOfBirthBox.GetBindingExpression(DatePicker.SelectedDateProperty)?.UpdateSource();
        }

        private void AddMemberView_Loaded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
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
