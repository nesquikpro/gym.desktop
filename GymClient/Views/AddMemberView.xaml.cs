using System.Windows;
using System.Windows.Input;
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
        }

        private void DateOfBirth_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null) return;

            string currentText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength);
            currentText = currentText.Insert(textBox.CaretIndex, e.Text);

            string numbers = new(currentText.Where(char.IsDigit).ToArray());

            if (numbers.Length > 2) numbers = numbers.Insert(2, "/");
            if (numbers.Length > 5) numbers = numbers.Insert(5, "/");
            if (numbers.Length > 10) numbers = numbers.Substring(0, 10);

            textBox.Text = numbers;
            textBox.CaretIndex = textBox.Text.Length;
            e.Handled = true;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
