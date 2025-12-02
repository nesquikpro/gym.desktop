using GymClient.Interfaces;
using GymClient.Services;
using GymClient.Utils;
using GymClient.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Windows;

namespace GymClient
{
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider Services { get; private set; } = null!;
        private AuthorizationView? _loginView;
        private MainWindow? _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ThemeManager.ApplyTheme();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var apiBaseUrl = configuration["Api:BaseUrl"];

            var services = new ServiceCollection();

            services.AddSingleton(new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

            services.AddSingleton<ApiClient>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IExportService, ExportService>();
            services.AddSingleton<CsvImportService>();
            services.AddSingleton<IQrCodeService, QrCodeService>();

            services.AddTransient<AuthViewModel>();

            services.AddSingleton<MemberViewModel>();
            services.AddSingleton<MembershipViewModel>();
            services.AddSingleton<ChipViewModel>();
            services.AddSingleton<VisitViewModel>();

            services.AddSingleton<MainViewModel>(sp =>
            {
                var membersVM = sp.GetRequiredService<MemberViewModel>();
                var membershipsVM = sp.GetRequiredService<MembershipViewModel>();
                var chipsVM = sp.GetRequiredService<ChipViewModel>();
                var visitsVM = sp.GetRequiredService<VisitViewModel>();

                return new MainViewModel(membersVM, membershipsVM, chipsVM, visitsVM);
            });

            services.AddTransient<MainWindow>();

            Services = services.BuildServiceProvider();
            ShowLoginWindow();
        }

        public void ShowLoginWindow()
        {
            var loginVM = Services.GetRequiredService<AuthViewModel>();
            _loginView = new AuthorizationView { DataContext = loginVM };

            loginVM.LoginSucceeded += OnLoginSucceeded;

            _loginView.Show();
            this.MainWindow = _loginView;
        }

        private void OnLoginSucceeded(string username)
        {
            var mainVM = Services.GetRequiredService<MainViewModel>();
            mainVM.CurrentUsername = username;

            _mainWindow = Services.GetRequiredService<MainWindow>();
            _mainWindow.DataContext = mainVM;

            mainVM.LogoutRequested += OnLogoutRequested;

            _mainWindow.Show();
            _loginView.Close();
        }

        private void OnLogoutRequested()
        {
            if (_mainWindow != null)
            {
                var mainVM = (MainViewModel)_mainWindow.DataContext!;
                mainVM.LogoutRequested -= OnLogoutRequested;

                ShowLoginWindow();

                _mainWindow.Close();
                _mainWindow = null;
            }
        }
    }
}
