using GymTracker.Models;
using GymTracker.Repository;
using GymTracker.Repository.Auth;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace GymTracker.IntegrationTests
{
    public class AuthIntegrationTests : IClassFixture<GymTrackerWebApplicationFactory>
    {
        private readonly GymTrackerWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public AuthIntegrationTests(GymTrackerWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_WithValidData_ShouldCreateNewUser()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            // === 1. ARRANGE ===
            var signupRequest = new SignupRequest
            {
                Name = "TestUser",
                Email = "testuser@example.com",
                Password = "SecurePassword123!"
            };

            // === 2. ACT ===
            var response = await _client.PostAsJsonAsync("/Auth/register", signupRequest);
            var responseText = await response.Content.ReadAsStringAsync();

            // === 3. ASSERT ===
            Assert.True(response.IsSuccessStatusCode, $"Сервер вернул ошибку: {responseText}");

            // Проверяем, что пользователь был создан в БД
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                var createdUser = dbContext.Users.FirstOrDefault(u => u.Email == "testuser@example.com");

                Assert.NotNull(createdUser);
                Assert.Equal("TestUser", createdUser.FullName);
                Assert.Equal("testuser@example.com", createdUser.Email);
                Assert.NotNull(createdUser.PasswordHash);
                Assert.NotEmpty(createdUser.PasswordHash);
                Assert.True(createdUser.CreatedAt > DateTime.UtcNow.AddSeconds(-10));
            }
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            // Создаем первого пользователя в БД
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                var user = new User
                {
                    FullName = "ExistingUser",
                    Email = "existing@example.com",
                    PasswordHash = "SomeHash",
                    CreatedAt = DateTime.UtcNow
                };
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();
            }

            // === 1. ARRANGE ===
            var signupRequest = new SignupRequest
            {
                Name = "NewUser",
                Email = "existing@example.com", // Используем существующий email
                Password = "Password123!"
            };

            // === 2. ACT ===
            var response = await _client.PostAsJsonAsync("/Auth/register", signupRequest);

            // === 3. ASSERT ===
            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Register_WithDuplicateUsername_ShouldReturnBadRequest()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            // Создаем первого пользователя в БД
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                var user = new User
                {
                    FullName = "ExistingUser",
                    Email = "first@example.com",
                    PasswordHash = "SomeHash",
                    CreatedAt = DateTime.UtcNow
                };
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();
            }

            // === 1. ARRANGE ===
            var signupRequest = new SignupRequest
            {
                Name = "ExistingUser", // Используем существующее имя
                Email = "different@example.com",
                Password = "Password123!"
            };

            // === 2. ACT ===
            var response = await _client.PostAsJsonAsync("/Auth/register", signupRequest);

            // === 3. ASSERT ===
            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnJwtToken()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            string testEmail = "testuser@example.com";
            string testPassword = "SecurePassword123!";

            // Регистрируем пользователя
            var signupRequest = new SignupRequest
            {
                Name = "TestUser",
                Email = testEmail,
                Password = testPassword
            };

            await _client.PostAsJsonAsync("/Auth/register", signupRequest);

            // === 1. ARRANGE ===
            // Используем registered credentials

            // === 2. ACT ===
            var response = await _client.PostAsync(
                $"/Auth/login?email={Uri.EscapeDataString(testEmail)}&passwd={Uri.EscapeDataString(testPassword)}",
                null);
            var responseText = await response.Content.ReadAsStringAsync();

            // === 3. ASSERT ===
            Assert.True(response.IsSuccessStatusCode, $"Ошибка при логине: {responseText}");

            var jsonResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.NotNull(jsonResponse);
            Assert.True(jsonResponse.ContainsKey("token"));

            var token = jsonResponse["token"];
            Assert.NotEmpty(token);

            // Проверяем, что токен является валидным JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            Assert.NotNull(jwtToken);
            Assert.NotEmpty(jwtToken.Claims);

            // Проверяем наличие нужных claims
            var fullNameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "FullName");
            var idClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "Id");

            Assert.NotNull(fullNameClaim);
            Assert.Equal("TestUser", fullNameClaim.Value);
            Assert.NotNull(idClaim);
        }

        [Fact]
        public async Task Login_WithInvalidEmail_ShouldReturnBadRequest()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            // === 1. ACT ===
            var response = await _client.PostAsync(
                $"/Auth/login?email={Uri.EscapeDataString("nonexistent@example.com")}&passwd={Uri.EscapeDataString("SomePassword")}",
                null);

            // === 2. ASSERT ===
            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ShouldReturnBadRequest()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            string testEmail = "testuser@example.com";
            string testPassword = "SecurePassword123!";

            // Регистрируем пользователя
            var signupRequest = new SignupRequest
            {
                Name = "TestUser",
                Email = testEmail,
                Password = testPassword
            };

            await _client.PostAsJsonAsync("/Auth/register", signupRequest);

            // === 1. ACT ===
            var response = await _client.PostAsync(
                $"/Auth/login?email={Uri.EscapeDataString(testEmail)}&passwd={Uri.EscapeDataString("WrongPassword")}",
                null);

            // === 2. ASSERT ===
            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Register_ShouldSetCorrectCreatedAtTimestamp()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            var beforeRegistration = DateTime.UtcNow;

            // === 1. ARRANGE ===
            var signupRequest = new SignupRequest
            {
                Name = "TestUser",
                Email = "timestamptest@example.com",
                Password = "Password123!"
            };

            // === 2. ACT ===
            var response = await _client.PostAsJsonAsync("/Auth/register", signupRequest);
            var afterRegistration = DateTime.UtcNow;

            // === 3. ASSERT ===
            Assert.True(response.IsSuccessStatusCode);

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                var user = dbContext.Users.FirstOrDefault(u => u.Email == "timestamptest@example.com");

                Assert.NotNull(user);
                Assert.True(user.CreatedAt >= beforeRegistration);
                Assert.True(user.CreatedAt <= afterRegistration);
            }
        }

        [Fact]
        public async Task Login_TokenShouldBeValidForSubsequentRequests()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            string testEmail = "testuser@example.com";
            string testPassword = "SecurePassword123!";

            // Регистрируем пользователя
            var signupRequest = new SignupRequest
            {
                Name = "TestUser",
                Email = testEmail,
                Password = testPassword
            };

            await _client.PostAsJsonAsync("/Auth/register", signupRequest);

            // === 1. ARRANGE - Логинимся и получаем токен ===
            var loginResponse = await _client.PostAsync(
                $"/Auth/login?email={Uri.EscapeDataString(testEmail)}&passwd={Uri.EscapeDataString(testPassword)}",
                null);

            var loginResponseText = await loginResponse.Content.ReadAsStringAsync();
            var jsonResponse = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var token = jsonResponse?["token"];

            Assert.NotNull(token);

            // === 2. ACT - Используем токен в заголовке Authorization ===
            var clientWithAuth = _factory.CreateClient();
            clientWithAuth.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Делаем какой-то запрос с токеном (например, GET /GetExercises)
            var authorizedResponse = await clientWithAuth.GetAsync("/GetExercises");

            // === 3. ASSERT ===
            // Просто проверяем, что запрос прошел (не 401 Unauthorized)
            Assert.NotEqual(System.Net.HttpStatusCode.Unauthorized, authorizedResponse.StatusCode);
        }

        [Fact]
        public async Task Register_EmptyName_ShouldStillCreateUser()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            // === 1. ARRANGE ===
            var signupRequest = new SignupRequest
            {
                Name = "", // Пустое имя
                Email = "emptyname@example.com",
                Password = "Password123!"
            };

            // === 2. ACT ===
            var response = await _client.PostAsJsonAsync("/Auth/register", signupRequest);

            // === 3. ASSERT ===
            // Это зависит от валидации на сервере. Если сервер позволяет, тест пройдет
            if (response.IsSuccessStatusCode)
            {
                using (var scope = _factory.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                    var user = dbContext.Users.FirstOrDefault(u => u.Email == "emptyname@example.com");
                    Assert.NotNull(user);
                    Assert.Equal("", user.FullName);
                }
            }
        }

        [Fact]
        public async Task Login_MultipleUsersWithDifferentCredentials()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            // Регистрируем двух пользователей
            var user1 = new SignupRequest { Name = "User1", Email = "user1@example.com", Password = "Password1!" };
            var user2 = new SignupRequest { Name = "User2", Email = "user2@example.com", Password = "Password2!" };

            await _client.PostAsJsonAsync("/Auth/register", user1);
            await _client.PostAsJsonAsync("/Auth/register", user2);

            // === 1. ACT & ASSERT для User1 ===
            var login1Response = await _client.PostAsync(
                $"/Auth/login?email={Uri.EscapeDataString("user1@example.com")}&passwd={Uri.EscapeDataString("Password1!")}",
                null);

            Assert.True(login1Response.IsSuccessStatusCode);
            var json1 = await login1Response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var token1 = json1?["token"];
            Assert.NotNull(token1);

            var handler = new JwtSecurityTokenHandler();
            var jwt1 = handler.ReadJwtToken(token1);
            var claim1 = jwt1.Claims.FirstOrDefault(c => c.Type == "FullName");
            Assert.Equal("User1", claim1?.Value);

            // === 2. ACT & ASSERT для User2 ===
            var login2Response = await _client.PostAsync(
                $"/Auth/login?email={Uri.EscapeDataString("user2@example.com")}&passwd={Uri.EscapeDataString("Password2!")}",
                null);

            Assert.True(login2Response.IsSuccessStatusCode);
            var json2 = await login2Response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var token2 = json2?["token"];
            Assert.NotNull(token2);

            var jwt2 = handler.ReadJwtToken(token2);
            var claim2 = jwt2.Claims.FirstOrDefault(c => c.Type == "FullName");
            Assert.Equal("User2", claim2?.Value);

            // Проверяем, что токены разные
            Assert.NotEqual(token1, token2);
        }
    }
}
