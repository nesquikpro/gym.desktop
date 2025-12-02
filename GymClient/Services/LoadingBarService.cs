using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GymClient.Services
{
    public class LoadingBarService : INotifyPropertyChanged
    {
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Singleton
        private static LoadingBarService? _instance;
        public static LoadingBarService Instance => _instance ??= new LoadingBarService();
    }
}
