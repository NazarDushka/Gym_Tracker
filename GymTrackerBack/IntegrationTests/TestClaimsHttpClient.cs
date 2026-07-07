using System.Security.Claims;
using System.Net.Http.Headers;

namespace IntegrationTests
{
    /// <summary>
    /// Helper class to add custom claims to test HTTP requests
    /// Allows emulating an authenticated user with specific claims
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

            // Set claims in TestAuthHandler
            ApplyClaims();
        }

        /// <summary>
        /// Adds an additional claim
        /// </summary>
        public TestClaimsHttpClient WithClaim(string claimType, string claimValue)
        {
            _claims[claimType] = claimValue;
            ApplyClaims();
            return this;
        }

        /// <summary>
        /// Applies claims to TestAuthHandler
        /// </summary>
        private void ApplyClaims()
        {
            // Clear old test headers if the client is reused
            var headersToRemove = _httpClient.DefaultRequestHeaders
                .Where(h => h.Key.StartsWith("X-Test-Claim-"))
                .Select(h => h.Key)
                .ToList();

            foreach (var header in headersToRemove)
            {
                _httpClient.DefaultRequestHeaders.Remove(header);
            }

            // Add new claims as HTTP headers
            foreach (var claim in _claims)
            {
                _httpClient.DefaultRequestHeaders.Add($"X-Test-Claim-{claim.Key}", claim.Value);
            }
        }

        /// <summary>
        /// Performs a GET request with the set claims
        /// </summary>
        public async Task<HttpResponseMessage> GetAsync(string uri)
        {
            return await _httpClient.GetAsync(uri);
        }

        /// <summary>
        /// Performs a POST request with the set claims
        /// </summary>
        public async Task<HttpResponseMessage> PostAsync(string uri, HttpContent content)
        {
            return await _httpClient.PostAsync(uri, content);
        }

        /// <summary>
        /// Performs a DELETE request with the set claims
        /// </summary>
        public async Task<HttpResponseMessage> DeleteAsync(string uri)
        {
            return await _httpClient.DeleteAsync(uri);
        }

        /// <summary>
        /// Performs a PUT request with the set claims
        /// </summary>
        public async Task<HttpResponseMessage> PutAsync(string uri, HttpContent content)
        {
            return await _httpClient.PutAsync(uri, content);
        }

        /// <summary>
        /// Gets current claims
        /// </summary>
        public IReadOnlyDictionary<string, string> GetClaims()
        {
            return _claims.AsReadOnly();
        }
    }
}

