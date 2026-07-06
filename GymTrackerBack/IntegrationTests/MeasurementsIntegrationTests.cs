using GymTracker.Models;
using GymTracker.Repository;
using IntegrationTests;
using Microsoft.Extensions.DependencyInjection;
using System;
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
            Guid testTypeId;

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

            Guid testTypeId;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                var type = new MeasurementType { Name = "Weight", Unit = "kg" };
                dbContext.MeasurementTypes.Add(type);
                await dbContext.SaveChangesAsync();
                testTypeId = type.Id;
            }

            // === 1. ARRANGE ===

            _client.DefaultRequestHeaders.Remove("X-Test-Claim-NameIdentifier");
            _client.DefaultRequestHeaders.Add("X-Test-Claim-NameIdentifier", "999");

            var newLogRequest = new
            {
                MeasurementTypeId = testTypeId,
                Value = 75.5f
            };

            // === 2. ACT ===
            var response = await _client.PostAsJsonAsync("/GymTracker/Measurements", newLogRequest);

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
                MeasurementTypeId = Guid.NewGuid(), // Несуществующий тип
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
            var response = await _client.GetAsync($"/GymTracker/Measurements/UsersMeasurements");
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

            Guid testLogId;

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
            Guid invalidLogId = Guid.NewGuid(); // Несуществующий лог   

            // === 1. ACT ===
            var response = await _client.DeleteAsync($"/GymTracker/Measurements/{invalidLogId}");

            // === 2. ASSERT ===
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddTarget_ShouldCreateNewTarget()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            int testUserId;
            Guid testTypeId;

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
            Guid testTypeId;
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
            var response = await _client.GetAsync($"/targets");
            var responseText = await response.Content.ReadAsStringAsync();

            // === 2. ASSERT ===
            Assert.True(response.IsSuccessStatusCode, $"Сервер упал с ошибкой: {responseText}");

            var targets = await response.Content.ReadFromJsonAsync<List<MeasurementTarget>>();
            Assert.NotNull(targets);
            Assert.Single(targets); 
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

        [Fact]
        public async Task GetLastLogsForUser_ShouldReturnLastLogForEachType()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            int testUserId;
            Guid type1Id;
            Guid type2Id;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                var user = new User { FullName = "TestUser", Email = "test@example.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
                var type1 = new MeasurementType { Name = "Weight", Unit = "kg" };
                var type2 = new MeasurementType { Name = "Body Fat Percentage", Unit = "%" };

                dbContext.Users.Add(user);
                dbContext.MeasurementTypes.AddRange(type1, type2);
                await dbContext.SaveChangesAsync();

                testUserId = user.Id;
                type1Id = type1.Id;
                type2Id = type2.Id;

                // Adding multiple logs for type1 (Weight)
                var log1 = new MeasurementLog 
                { 
                    UserId = user.Id, 
                    MeasurementTypeId = type1.Id, 
                    Value = 75.0f, 
                    Date = DateTime.UtcNow.AddDays(-2) 
                };
                var log2 = new MeasurementLog 
                { 
                    UserId = user.Id, 
                    MeasurementTypeId = type1.Id, 
                    Value = 74.5f, 
                    Date = DateTime.UtcNow.AddDays(-1) 
                };
                var log3 = new MeasurementLog 
                { 
                    UserId = user.Id, 
                    MeasurementTypeId = type1.Id, 
                    Value = 74.0f, 
                    Date = DateTime.UtcNow 
                };

                // Adding multiple logs for type2 (Body Fat Percentage) 
                var log4 = new MeasurementLog 
                { 
                    UserId = user.Id, 
                    MeasurementTypeId = type2.Id, 
                    Value = 20.0f, 
                    Date = DateTime.UtcNow.AddDays(-1) 
                };
                var log5 = new MeasurementLog 
                { 
                    UserId = user.Id, 
                    MeasurementTypeId = type2.Id, 
                    Value = 19.5f, 
                    Date = DateTime.UtcNow 
                };

                dbContext.MeasurementLogs.AddRange(log1, log2, log3, log4, log5);
                await dbContext.SaveChangesAsync();
            }

            // === 1. ACT ===

            var response = await _client.GetAsync("/GymTracker/Measurements/last");
            var responseText = await response.Content.ReadAsStringAsync();

            // === 2. ASSERT ===
            Assert.True(response.IsSuccessStatusCode, $"Сервер упал с ошибкой: {responseText}");

            var logs = await response.Content.ReadFromJsonAsync<List<MeasurementLog>>();
            Assert.NotNull(logs);
            // should return last log for each type, so we expect 2 logs (one for Weight and one for Body Fat Percentage)
            Assert.Equal(2, logs.Count);

            // Проверяем что это последние логи
            var lastLog1 = logs.FirstOrDefault(l => l.MeasurementTypeId == type1Id);
            var lastLog2 = logs.FirstOrDefault(l => l.MeasurementTypeId == type2Id);

            Assert.NotNull(lastLog1);
            Assert.Equal(74.0f, lastLog1.Value); // last weight log

            Assert.NotNull(lastLog2);
            Assert.Equal(19.5f, lastLog2.Value); // last body fat percentage log
        }

        [Fact]
        public async Task GetLastLogsForUser_WithNoLogs_ShouldReturnNotFound()
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

            // === 1. ACT ===
            var response = await _client.GetAsync("/GymTracker/Measurements/last");

            // === 2. ASSERT ===
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetLastLogsForUser_ShouldReturnOnlyForCurrentUser()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            int testUserId1;
            int testUserId2;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                var user1 = new User { FullName = "TestUser1", Email = "test1@example.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
                var user2 = new User { FullName = "TestUser2", Email = "test2@example.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
                var type = new MeasurementType { Name = "Weight", Unit = "kg" };

                dbContext.Users.AddRange(user1, user2);
                dbContext.MeasurementTypes.Add(type);
                await dbContext.SaveChangesAsync();

                testUserId1 = user1.Id;
                testUserId2 = user2.Id;

                // Логи для первого пользователя
                var log1 = new MeasurementLog { UserId = user1.Id, MeasurementTypeId = type.Id, Value = 75.0f, Date = DateTime.UtcNow };
                var log2 = new MeasurementLog { UserId = user1.Id, MeasurementTypeId = type.Id, Value = 74.0f, Date = DateTime.UtcNow.AddDays(-1) };

                // Логи для второго пользователя
                var log3 = new MeasurementLog { UserId = user2.Id, MeasurementTypeId = type.Id, Value = 80.0f, Date = DateTime.UtcNow };
                var log4 = new MeasurementLog { UserId = user2.Id, MeasurementTypeId = type.Id, Value = 79.0f, Date = DateTime.UtcNow.AddDays(-1) };
                dbContext.MeasurementLogs.AddRange(log1, log2, log3, log4);
                await dbContext.SaveChangesAsync();
            }

            // === 1. ACT - Получаем логи первого пользователя ===
            _client.DefaultRequestHeaders.Remove("X-Test-Claim-NameIdentifier");
            _client.DefaultRequestHeaders.Add("X-Test-Claim-NameIdentifier", testUserId1.ToString());

            var response1 = await _client.GetAsync("/GymTracker/Measurements/last");

            // === 2. ASSERT ===
            Assert.True(response1.IsSuccessStatusCode);

            var logs = await response1.Content.ReadFromJsonAsync<List<MeasurementLog>>();
            Assert.NotNull(logs);
            Assert.Single(logs); // only one log should be returned for the last measurement of the first user

            // Check that the returned log belongs to the first user and has the correct value
            Assert.Equal(75.0f, logs[0].Value);
        }
    }
}


