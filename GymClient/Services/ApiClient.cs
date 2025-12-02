using GymClient.Models;
using GymClient.Services.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;

namespace GymClient.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly int _maxRetries = 3;
        private readonly TimeSpan _baseDelay = TimeSpan.FromSeconds(1);

        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task<ApiResponse<T>> SafeRequest<T>(Func<Task<HttpResponseMessage>> httpCall)
        {
            int attempt = 0;
            try
            {
                LoadingBarService.Instance.IsLoading = true;

                while (true)
                {
                    try
                    {
                        var response = await httpCall();

                        if (!response.IsSuccessStatusCode)
                        {
                            var errorMsg = $"HTTP {(int)response.StatusCode} - {response.ReasonPhrase}";
                            LogError(errorMsg);
                            return new ApiResponse<T> { IsSuccess = false, ErrorMessage = errorMsg };
                        }

                        var json = await response.Content.ReadAsStringAsync();

                        if (string.IsNullOrWhiteSpace(json))
                        {
                            if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
                            {
                                return new ApiResponse<T> { IsSuccess = true, Data = default! };
                            }
                            else
                            {
                                return new ApiResponse<T> { IsSuccess = true, Data = default };
                            }
                        }

                        var data = JsonConvert.DeserializeObject<T>(json)!;

                        return new ApiResponse<T> { IsSuccess = true, Data = data };
                    }
                    catch (HttpRequestException ex) when (attempt < _maxRetries)
                    {
                        attempt++;
                        var delay = TimeSpan.FromMilliseconds(_baseDelay.TotalMilliseconds * Math.Pow(2, attempt));
                        LogError($"Попытка {attempt}: временная ошибка сети: {ex.Message}. Повтор через {delay.TotalSeconds} сек.");
                        await Task.Delay(delay);
                    }
                    catch (Exception ex)
                    {
                        LogError($"Ошибка запроса: {ex.Message}");
                        return new ApiResponse<T> { IsSuccess = false, ErrorMessage = ex.Message };
                    }
                }
            }
            finally
            {
                LoadingBarService.Instance.IsLoading = false;
            }
        }

        private void LogError(string message)
        {
            Console.WriteLine($"[ApiClient Error] {message}");
        }

        // GET ALL
        public Task<ApiResponse<List<TObj>>> GetAll<TObj>(string path) where TObj : ModelAbstract
        {
            return SafeRequest<List<TObj>>(() => _httpClient.GetAsync($"{path}"));
        }

        // GET BY ID
        public Task<ApiResponse<TObj>> GetById<TObj>(TObj obj) where TObj : ModelAbstract
        {
            return SafeRequest<TObj>(() => _httpClient.GetAsync($"{obj.Path}/{obj.Id}"));
        }

        // POST
        public Task<ApiResponse<TObj>> Post<TObj>(TObj obj) where TObj : ModelAbstract
        {
            var content = new StringContent(obj.ToJson(), Encoding.UTF8, "application/json");
            return SafeRequest<TObj>(() => _httpClient.PostAsync(obj.Path, content));
        }

        // PUT/UPDATE
        public Task<ApiResponse<TObj>> Update<TObj>(TObj obj) where TObj : ModelAbstract
        {
            var content = new StringContent(obj.ToJson(), Encoding.UTF8, "application/json");
            return SafeRequest<TObj>(() => _httpClient.PutAsync($"{obj.Path}/{obj.Id}", content));
        }

        // DELETE
        public Task<ApiResponse<bool>> Delete<TObj>(TObj obj) where TObj : ModelAbstract
        {
            return SafeRequest<bool>(async () =>
            {
                var response = await _httpClient.DeleteAsync($"{obj.Path}/{obj.Id}");

                if (response.Content == null || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    response.Content = new StringContent("true", Encoding.UTF8, "application/json");
                }

                return response;
            });
        }

        // POST AUTH
        public async Task<ApiResponse<LoginResponse>> PostAuth(string uri, object payload)
        {
            return await SafeRequest<LoginResponse>(() => _httpClient.PostAsJsonAsync(uri, payload));
        }

        public Task<ApiResponse<HttpResponseMessage>> PostPaymentEmailAsync(string uri, object payload)
        {
            return SafeRequest<HttpResponseMessage>(() => _httpClient.PostAsJsonAsync(uri, payload));
        }
    }
}
