using GymTracker.Models;
using GymTracker.Repository;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace GymTracker.IntegrationTests
{
    public class MeasurementsIntegrationTests : IClassFixture<GymTrackerWebApplicationFactory>
    {
        private readonly GymTrackerWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public MeasurementsIntegrationTests(GymTrackerWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
        }

        [Fact]
        public async Task GetMeasurementTypes_ShouldReturnAllTypes()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                var type1 = new MeasurementType { Name = "Body Fat Percentage", Unit = "%" };
                var type2 = new MeasurementType { Name = "Weight", Unit = "kg" };
                var type3 = new MeasurementType { Name = "Height", Unit = "cm" };

                dbContext.MeasurementTypes.AddRange(type1, type2, type3);
                await dbContext.SaveChangesAsync();
            }

            // === 1. ACT ===
            var response = await _client.GetAsync("/GymTracker/Measurements/types");
            var responseText = await response.Content.ReadAsStringAsync();

            // === 2. ASSERT ===
            Assert.True(response.IsSuccessStatusCode, $"Сервер упал с ошибкой: {responseText}");

            var types = await response.Content.ReadFromJsonAsync<List<MeasurementType>>();
            Assert.NotNull(types);
            Assert.Equal(3, types.Count);
            Assert.Contains(types, t => t.Name == "Body Fat Percentage");
            Assert.Contains(types, t => t.Name == "Weight");
            Assert.Contains(types, t => t.Name == "Height");
        }

        [Fact]
        public async Task AddMeasurementLog_WithValidData_ShouldCreateLog()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            int testUserId;
            int testTypeId;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                var user = new User { FullName = "TestUser", Email = "test@example.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
                var type = new MeasurementType { Name = "Weight", Unit = "kg" };

                dbContext.Users.Add(user);
                dbContext.MeasurementTypes.Add(type);
                await dbContext.SaveChangesAsync();

                testUserId = user.Id;
                testTypeId = type.Id;
            }

            // === 1. ARRANGE ===
            var newLog = new MeasurementLog
            {
                UserId = testUserId,
                MeasurementTypeId = testTypeId,
                Value = 75.5f,
                Date = DateTime.UtcNow
            };

            // === 2. ACT ===
            var response = await _client.PostAsJsonAsync("/GymTracker/Measurements", newLog);
            var responseText = await response.Content.ReadAsStringAsync();

            // === 3. ASSERT ===
            Assert.True(response.IsSuccessStatusCode, $"Сервер упал с ошибкой: {responseText}");
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

            // Проверяем, что лог был сохранен в БД
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                var savedLog = dbContext.MeasurementLogs.FirstOrDefault(l => l.UserId == testUserId && l.Value == 75.5f);

                Assert.NotNull(savedLog);
                Assert.Equal(testTypeId, savedLog.MeasurementTypeId);
                Assert.Equal(75.5f, savedLog.Value);
            }
        }

        [Fact]
        public async Task AddMeasurementLog_WithInvalidUserId_ShouldReturnBadRequest()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            int testTypeId;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                var type = new MeasurementType { Name = "Weight", Unit = "kg" };
                dbContext.MeasurementTypes.Add(type);
                await dbContext.SaveChangesAsync();
                testTypeId = type.Id;
            }

            // === 1. ARRANGE ===
            var newLog = new MeasurementLog
            {
                UserId = 999, // Несуществующий пользователь
                MeasurementTypeId = testTypeId,
                Value = 75.5f
            };

            // === 2. ACT ===
            var response = await _client.PostAsJsonAsync("/GymTracker/Measurements", newLog);

            // === 3. ASSERT ===
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddMeasurementLog_WithInvalidTypeId_ShouldReturnBadRequest()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            int testUserId;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                var user = new User { FullName = "TestUser", Email = "test@example.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();
                testUserId = user.Id;
            }

            // === 1. ARRANGE ===
            var newLog = new MeasurementLog
            {
                UserId = testUserId,
                MeasurementTypeId = 999, // Несуществующий тип
                Value = 75.5f
            };

            // === 2. ACT ===
            var response = await _client.PostAsJsonAsync("/GymTracker/Measurements", newLog);

            // === 3. ASSERT ===
            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task GetUserLogs_ShouldReturnUserMeasurements()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            int testUserId;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                var user = new User { FullName = "TestUser", Email = "test@example.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
                var type = new MeasurementType { Name = "Weight", Unit = "kg" };

                dbContext.Users.Add(user);
                dbContext.MeasurementTypes.Add(type);
                await dbContext.SaveChangesAsync();

                var log1 = new MeasurementLog { UserId = user.Id, MeasurementTypeId = type.Id, Value = 75.5f, Date = DateTime.UtcNow };
                var log2 = new MeasurementLog { UserId = user.Id, MeasurementTypeId = type.Id, Value = 76.0f, Date = DateTime.UtcNow.AddDays(1) };

                dbContext.MeasurementLogs.AddRange(log1, log2);
                await dbContext.SaveChangesAsync();

                testUserId = user.Id;
            }

            // === 1. ACT ===
            var response = await _client.GetAsync($"/api/users/{testUserId}/measurements");
            var responseText = await response.Content.ReadAsStringAsync();

            // === 2. ASSERT ===
            Assert.True(response.IsSuccessStatusCode, $"Сервер упал с ошибкой: {responseText}");

            var logs = await response.Content.ReadFromJsonAsync<List<MeasurementLog>>();
            Assert.NotNull(logs);
            Assert.Equal(2, logs.Count);
        }

        [Fact]
        public async Task GetUserLogs_WithInvalidUserId_ShouldReturnNotFound()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            // === 1. ACT ===
            var response = await _client.GetAsync($"/api/users/999/measurements");

            // === 2. ASSERT ===
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteMeasurementLog_ShouldRemoveLog()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            int testLogId;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                var user = new User { FullName = "TestUser", Email = "test@example.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
                var type = new MeasurementType { Name = "Weight", Unit = "kg" };

                dbContext.Users.Add(user);
                dbContext.MeasurementTypes.Add(type);
                await dbContext.SaveChangesAsync();

                var log = new MeasurementLog { UserId = user.Id, MeasurementTypeId = type.Id, Value = 75.5f, Date = DateTime.UtcNow };
                dbContext.MeasurementLogs.Add(log);
                await dbContext.SaveChangesAsync();

                testLogId = log.Id;
            }

            // === 1. ACT ===
            var response = await _client.DeleteAsync($"/GymTracker/Measurements/{testLogId}");
            var responseText = await response.Content.ReadAsStringAsync();

            // === 2. ASSERT ===
            Assert.True(response.IsSuccessStatusCode, $"Сервер упал с ошибкой: {responseText}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);

            // Проверяем, что лог был удален из БД
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                var deletedLog = dbContext.MeasurementLogs.FirstOrDefault(l => l.Id == testLogId);
                Assert.Null(deletedLog);
            }
        }

        [Fact]
        public async Task DeleteMeasurementLog_WithInvalidId_ShouldReturnNotFound()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            // === 1. ACT ===
            var response = await _client.DeleteAsync($"/GymTracker/Measurements/999");

            // === 2. ASSERT ===
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddTarget_ShouldCreateNewTarget()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            int testUserId;
            int testTypeId;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                var user = new User { FullName = "TestUser", Email = "test@example.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
                var type = new MeasurementType { Name = "Weight", Unit = "kg" };

                dbContext.Users.Add(user);
                dbContext.MeasurementTypes.Add(type);
                await dbContext.SaveChangesAsync();

                testUserId = user.Id;
                testTypeId = type.Id;
            }

            // === 1. ARRANGE ===
            var newTarget = new MeasurementTarget
            {
                UserId = testUserId,
                MeasurementTypeId = testTypeId,
                TargetValue = 70.0f,
                Deadline = DateTime.UtcNow.AddMonths(3)
            };

            // === 2. ACT ===
            var response = await _client.PostAsJsonAsync("/GymTracker/Measurements/targets", newTarget);
            var responseText = await response.Content.ReadAsStringAsync();

            // === 3. ASSERT ===
            Assert.True(response.IsSuccessStatusCode, $"Сервер упал с ошибкой: {responseText}");
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

            // Проверяем, что target был сохранен в БД
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                var savedTarget = dbContext.MeasurementTargets.FirstOrDefault(t => t.UserId == testUserId && t.TargetValue == 70.0f);

                Assert.NotNull(savedTarget);
                Assert.Equal(testTypeId, savedTarget.MeasurementTypeId);
                Assert.True(savedTarget.IsActive);
            }
        }

        [Fact]
        public async Task AddTarget_ShouldDeactivateOldTarget()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            int testUserId;
            int testTypeId;
            int oldTargetId;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                var user = new User { FullName = "TestUser", Email = "test@example.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
                var type = new MeasurementType { Name = "Weight", Unit = "kg" };

                dbContext.Users.Add(user);
                dbContext.MeasurementTypes.Add(type);
                await dbContext.SaveChangesAsync();

                var oldTarget = new MeasurementTarget
                {
                    UserId = user.Id,
                    MeasurementTypeId = type.Id,
                    TargetValue = 75.0f,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.MeasurementTargets.Add(oldTarget);
                await dbContext.SaveChangesAsync();

                testUserId = user.Id;
                testTypeId = type.Id;
                oldTargetId = oldTarget.Id;
            }

            // === 1. ARRANGE ===
            var newTarget = new MeasurementTarget
            {
                UserId = testUserId,
                MeasurementTypeId = testTypeId,
                TargetValue = 70.0f
            };

            // === 2. ACT ===
            var response = await _client.PostAsJsonAsync("/GymTracker/Measurements/targets", newTarget);

            // === 3. ASSERT ===
            Assert.True(response.IsSuccessStatusCode,$"Ошибка:{response}");


            // Проверяем, что старый target был деактивирован
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                var deactivatedTarget = dbContext.MeasurementTargets.FirstOrDefault(t => t.Id == oldTargetId);
                var newCreatedTarget = dbContext.MeasurementTargets.FirstOrDefault(t => t.TargetValue == 70.0f);

                Assert.NotNull(deactivatedTarget);
                Assert.False(deactivatedTarget.IsActive);
                Assert.NotNull(newCreatedTarget);
                Assert.True(newCreatedTarget.IsActive);
            }
        }

        [Fact]
        public async Task GetUserTargets_ShouldReturnActiveTargets()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            int testUserId;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                var user = new User { FullName = "TestUser", Email = "test@example.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
                var type = new MeasurementType { Name = "Weight", Unit = "kg" };

                dbContext.Users.Add(user);
                dbContext.MeasurementTypes.Add(type);
                await dbContext.SaveChangesAsync();

                var activeTarget = new MeasurementTarget
                {
                    UserId = user.Id,
                    MeasurementTypeId = type.Id,
                    TargetValue = 70.0f,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var inactiveTarget = new MeasurementTarget
                {
                    UserId = user.Id,
                    MeasurementTypeId = type.Id,
                    TargetValue = 75.0f,
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.MeasurementTargets.AddRange(activeTarget, inactiveTarget);
                await dbContext.SaveChangesAsync();

                testUserId = user.Id;
            }

            // === 1. ACT ===
            var response = await _client.GetAsync($"/api/users/{testUserId}/targets");
            var responseText = await response.Content.ReadAsStringAsync();

            // === 2. ASSERT ===
            Assert.True(response.IsSuccessStatusCode, $"Сервер упал с ошибкой: {responseText}");

            var targets = await response.Content.ReadFromJsonAsync<List<MeasurementTarget>>();
            Assert.NotNull(targets);
            Assert.Single(targets); // Только активный target
            Assert.True(targets[0].IsActive);
            Assert.Equal(70.0f, targets[0].TargetValue);
        }

        [Fact]
        public async Task DeactivateTarget_ShouldMarkTargetAsInactive()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            int testTargetId;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                var user = new User { FullName = "TestUser", Email = "test@example.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
                var type = new MeasurementType { Name = "Weight", Unit = "kg" };

                dbContext.Users.Add(user);
                dbContext.MeasurementTypes.Add(type);
                await dbContext.SaveChangesAsync();

                var target = new MeasurementTarget
                {
                    UserId = user.Id,
                    MeasurementTypeId = type.Id,
                    TargetValue = 70.0f,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.MeasurementTargets.Add(target);
                await dbContext.SaveChangesAsync();

                testTargetId = target.Id;
            }

            // === 1. ACT ===
            var response = await _client.DeleteAsync($"/GymTracker/Measurements/targets/{testTargetId}");
            var responseText = await response.Content.ReadAsStringAsync();

            // === 2. ASSERT ===
            Assert.True(response.IsSuccessStatusCode, $"Сервер упал с ошибкой: {responseText}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);

            // Проверяем, что target был деактивирован
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                var deactivatedTarget = dbContext.MeasurementTargets.FirstOrDefault(t => t.Id == testTargetId);

                Assert.NotNull(deactivatedTarget);
                Assert.False(deactivatedTarget.IsActive);
            }
        }
    }
}
