using GymClient.Interfaces;
using GymClient.Utils;
using System.Windows.Input;

namespace GymClient.ViewModels
{
    public class AuthViewModel: BaseViewModel
    {
        private string _username = "admin";
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        private string _password = "admin";
        public string Password
        {
            private get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public ICommand LoginCommand { get; }

        public event Action<string>? LoginSucceeded;

        private readonly IAuthService _authService;

        public readonly IDialogService _dialogService;

        public AuthViewModel(IAuthService authService, IDialogService dialogService)
        {
            _authService = authService;
            _dialogService = dialogService;

            LoginCommand = new RelayCommand(async () => await LoginAsync());
        }

        private async Task LoginAsync()
        {
            var response = await _authService.LoginAsync(Username, Password);

            if (!response.IsSuccess)
            {
                await _dialogService.ShowMessage("Ошибка авторизации! \n" + response.ErrorMessage);
                return;
            }

            LoginSucceeded?.Invoke(Username);
        }
    }
}
