using GymTracker.Models;
using GymTracker.Repository;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace GymTracker.IntegrationTests
{
    public class WorkoutIntegrationTests : IClassFixture<GymTrackerWebApplicationFactory>
    {
        private readonly GymTrackerWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public WorkoutIntegrationTests(GymTrackerWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
        }

        [Fact]
        public async Task AddWorkout_ShouldSaveWorkoutAndCreatePersonalRecord()
        {
            int testExerciseId;
            int testUserId;

            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);
            
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                // Создаем сущности БЕЗ жестко заданных ID (пусть БД сгенерирует их сама)
                var exercise = new Exercise { Name = "Bench Press" };
                var user = new User { FullName = "TestUser", PasswordHash = "123", Email = "test@test.com" };

                dbContext.Exercises.Add(exercise);
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();

                // Забираем реальные ID, которые БД только что придумала!
                testExerciseId = exercise.Id;
                testUserId = user.Id;
            }

            // === 1. ARRANGE ===
            var newWorkout = new Workout
            {
                UserId = testUserId, // Используем реальный ID
                Date = DateTime.UtcNow,
                Notes = "Integration Test Workout",
                Sets = new List<WorkoutSet>
        {
            // Используем реальный ID упражнения
            new WorkoutSet { ExerciseId = testExerciseId, Weight = 100, Reps = 5 }
        }
            };

            // === 2. ACT ===
            var response = await _client.PostAsJsonAsync("/GymTracker/Workout/AddWorkout", newWorkout);
            var errorText = await response.Content.ReadAsStringAsync();

            // === 3. ASSERT ===
            Assert.True(response.IsSuccessStatusCode, $"Сервер упал с ошибкой: {errorText}");

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                var savedWorkout = dbContext.Workouts.FirstOrDefault(w => w.Notes == "Integration Test Workout");
                Assert.NotNull(savedWorkout);

                // Проверяем рекорд по нашим динамическим ID
                var personalRecord = dbContext.PersonalRecords.FirstOrDefault(pr => pr.UserId == testUserId && pr.ExerciseId == testExerciseId);

                Assert.NotNull(personalRecord);
                Assert.Equal(100 * (1 + 5 / 30.0f), personalRecord.CalculatedMaxLift);
            }
        }

        [Fact]
        public async Task GettingAndAddingMeasurements_ShouldSaveAndReturnLogs()
        {
            int testUserId;
            Guid testTypeId;

            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);
            
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                // Создаем тестовые данные
                var user = new User { FullName = "MeasurementTester", PasswordHash = "123", Email = "test@measurements.com" };
                var measurementType = new MeasurementType { Id = Guid.NewGuid(), Name = "Body Fat Percentage", Unit = "%" };

                dbContext.Users.Add(user);
                dbContext.MeasurementTypes.Add(measurementType);
                await dbContext.SaveChangesAsync();

                testUserId = user.Id;
                testTypeId = measurementType.Id;
            }

            // === 1. ARRANGE ===
            var newLog = new MeasurementLog
            {
                UserId = testUserId,
                MeasurementTypeId = testTypeId,
                Value = 15.5f,
                Date = DateTime.UtcNow
            };

            // === 2. ACT (POST - Добавляем замер) ===
            
            var postResponse = await _client.PostAsJsonAsync("/GymTracker/Measurements", newLog);
            var postErrorText = await postResponse.Content.ReadAsStringAsync();

            // === 3. ASSERT (POST) ===
            Assert.True(postResponse.IsSuccessStatusCode, $"Сервер вернул ошибку при POST: {postErrorText}");

            // === 4. ACT (GET - Получаем замеры обратно) ===
            var getResponse = await _client.GetAsync($"/api/users/{testUserId}/measurements");
            var getErrorText = await getResponse.Content.ReadAsStringAsync();

            // === 5. ASSERT (GET) ===
            Assert.True(getResponse.IsSuccessStatusCode, $"Сервер вернул ошибку при GET: {getErrorText}");

            // Десериализуем ответ от сервера в список логов
            var retrievedLogs = await getResponse.Content.ReadFromJsonAsync<List<MeasurementLog>>();

            // Проверяем, что данные дошли без искажений
            Assert.NotNull(retrievedLogs);
            Assert.Single(retrievedLogs); // Убеждаемся, что в базе ровно 1 запись, которую мы только что добавили
            Assert.Equal(15.5f, retrievedLogs[0].Value);
            Assert.Equal(testTypeId, retrievedLogs[0].MeasurementTypeId);
            Assert.Equal(testUserId, retrievedLogs[0].UserId);
        }
    }
}