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
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();

                // Создаем сущности БЕЗ жестко заданных ID (пусть БД сгенерирует их сама)
                var exercise = new Exercise { Name = "Bench Press" };
                var user = new User { FullName = "TestUser", PasswordHash = "123", Email = "test@test.com" };

                dbContext.Exercises.Add(exercise);
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();

                // Забираем реальные ID, которые БД только что придумала!
                testExerciseId = exercise.Id;
                testUserId = (int)user.Id;
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
    }
}