using GymClient.Models;

namespace GymClient.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<bool>> LoginAsync(string username, string password);
    }
}
