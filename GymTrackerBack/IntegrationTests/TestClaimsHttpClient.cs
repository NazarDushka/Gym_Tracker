using System.Security.Claims;
using System.Net.Http.Headers;

namespace IntegrationTests
{
    /// <summary>
    /// Вспомогательный класс для добавления custom claims в тестовые HTTP запросы
    /// Позволяет эмулировать аутентифицированного пользователя с определенными claims
    /// </summary>
    public class TestClaimsHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _claims;

        public TestClaimsHttpClient(HttpClient httpClient, int userId)
        {
            _httpClient = httpClient;
            _claims = new Dictionary<string, string>
            {
                { ClaimTypes.Name, "Test User" },
                { ClaimTypes.NameIdentifier, userId.ToString() },
                { "UserId", userId.ToString() },
                { "Id", userId.ToString() }
            };

            // Устанавливаем claims в TestAuthHandler
            ApplyClaims();
        }

        /// <summary>
        /// Добавляет дополнительный claim
        /// </summary>
        public TestClaimsHttpClient WithClaim(string claimType, string claimValue)
        {
            _claims[claimType] = claimValue;
            ApplyClaims();
            return this;
        }

        /// <summary>
        /// Применяет claims к TestAuthHandler
        /// </summary>
        private void ApplyClaims()
        {
            TestAuthHandler.CustomClaims = new Dictionary<string, string>(_claims);
        }

        /// <summary>
        /// Выполняет GET запрос с установленными claims
        /// </summary>
        public async Task<HttpResponseMessage> GetAsync(string uri)
        {
            return await _httpClient.GetAsync(uri);
        }

        /// <summary>
        /// Выполняет POST запрос с установленными claims
        /// </summary>
        public async Task<HttpResponseMessage> PostAsync(string uri, HttpContent content)
        {
            return await _httpClient.PostAsync(uri, content);
        }

        /// <summary>
        /// Выполняет DELETE запрос с установленными claims
        /// </summary>
        public async Task<HttpResponseMessage> DeleteAsync(string uri)
        {
            return await _httpClient.DeleteAsync(uri);
        }

        /// <summary>
        /// Выполняет PUT запрос с установленными claims
        /// </summary>
        public async Task<HttpResponseMessage> PutAsync(string uri, HttpContent content)
        {
            return await _httpClient.PutAsync(uri, content);
        }

        /// <summary>
        /// Получает текущие claims
        /// </summary>
        public IReadOnlyDictionary<string, string> GetClaims()
        {
            return _claims.AsReadOnly();
        }
    }
}

