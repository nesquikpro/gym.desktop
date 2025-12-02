using GymClient.Interfaces;
using GymClient.Models;

namespace GymClient.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApiClient _apiClient;
        public string? JwtToken { get; private set; }

        public AuthService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<bool>> LoginAsync(string username, string password)
        {
            var payload = new Login
            {
                Username = username,
                Password = password
            };

            var response = await _apiClient.PostAuth("api/auth/login", payload);

            if (response.IsSuccess )
            {
                return new ApiResponse<bool> { IsSuccess = true};
            }

            return new ApiResponse<bool>
            {
                IsSuccess = false,
                ErrorMessage = response.ErrorMessage ?? "Login failed"
            };
        }
    }
}
