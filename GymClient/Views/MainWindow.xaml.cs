using GymClient.Utils;
using GymClient.Views;
using System.Windows;

namespace GymClient
{
    public partial class MainWindow : Window
    {      
        private MemberView _memberView;
        private MembershipView _membershipView;
        private ChipView _chipView;
        private VisitView _visitView;
        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
        }
        private void LoadSettings()
        {
            var currentTheme = Properties.Settings.Default.AppTheme;
            if (string.IsNullOrEmpty(currentTheme) || currentTheme.Equals("Light", StringComparison.OrdinalIgnoreCase))
            {
                ThemeToggleButton.IsChecked = false;
            }
            else
            {
                ThemeToggleButton.IsChecked = true;
            }
        }
        private void ThemeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ApplyTheme("Dark");
        }

        private void ThemeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplyTheme("Light");
        }

        private void ApplyTheme(string theme)
        {
            if (!string.IsNullOrEmpty(theme))
            {
                ThemeManager.ChangeTheme(theme);

                var baseTheme = theme.Equals("Dark", StringComparison.OrdinalIgnoreCase)
                    ? MaterialDesignThemes.Wpf.BaseTheme.Dark
                    : MaterialDesignThemes.Wpf.BaseTheme.Light;
                ThemeManager.SetBaseTheme(baseTheme);

                Properties.Settings.Default.AppTheme = theme;
                Properties.Settings.Default.Save();
            }
        }

        private void OpenMembers_Command(object sender, RoutedEventArgs e)
        {
            if (_memberView == null)
                _memberView = new MemberView();
            MainContent.Content = _memberView;  
        }

        private void OpenMemberships_Command(object sender, RoutedEventArgs e)
        {
            if (_membershipView == null)
                _membershipView = new MembershipView();
            MainContent.Content = _membershipView;
        }

        private void OpenChips_Command(object sender, RoutedEventArgs e)
        {
            if (_chipView == null)
                _chipView = new ChipView();
            MainContent.Content = _chipView;
        }

        private void OpenVisits_Command(object sender, RoutedEventArgs e)
        {
            if (_visitView == null)
                _visitView = new VisitView();
            MainContent.Content = _visitView;
        }
    }
}