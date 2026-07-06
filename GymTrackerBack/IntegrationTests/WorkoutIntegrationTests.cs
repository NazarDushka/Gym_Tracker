using GymTracker.Models;
using GymTracker.Repository;
using Microsoft.EntityFrameworkCore;
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

                var exercise = new Exercise { Name = "Bench Press" };
                var user = new User { FullName = "TestUser", PasswordHash = "123", Email = "test@test.com" };

                dbContext.Exercises.Add(exercise);
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();

                testExerciseId = exercise.Id;
                testUserId = user.Id;
            }

            // === 1. ARRANGE ===
            _client.DefaultRequestHeaders.Remove("X-Test-Claim-NameIdentifier");
            _client.DefaultRequestHeaders.Add("X-Test-Claim-NameIdentifier", testUserId.ToString());

            var newWorkoutRequest = new
            {
                Date = DateTime.UtcNow,
                Notes = "Integration Test Workout",
                WorkoutSets = new[]
                {
            new { ExerciseId = testExerciseId, Weight = 100, Reps = 5 }
        }
            };

            // === 2. ACT ===
            var response = await _client.PostAsJsonAsync("/GymTracker/Workout/AddWorkout", newWorkoutRequest);
            var errorText = await response.Content.ReadAsStringAsync();

            // === 3. ASSERT  ===
            Assert.True(response.IsSuccessStatusCode, $"Сервер упал с ошибкой: {errorText}");

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                var savedWorkout = dbContext.Workouts
                    .Include(w => w.Sets)
                    .FirstOrDefault(w => w.Notes == "Integration Test Workout");

                Assert.NotNull(savedWorkout);
                Assert.NotNull(savedWorkout.Sets);
                Assert.Single(savedWorkout.Sets); 

                var personalRecord = dbContext.PersonalRecords
                    .FirstOrDefault(pr => pr.UserId == testUserId && pr.ExerciseId == testExerciseId);

                Assert.NotNull(personalRecord);

                float expectedMaxLift = 100 * (1 + 5 / 30.0f);
                Assert.Equal(expectedMaxLift, personalRecord.CalculatedMaxLift, 2);
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

                var user = new User { FullName = "MeasurementTester", PasswordHash = "123", Email = "test@measurements.com" };
                var measurementType = new MeasurementType { Id = Guid.NewGuid(), Name = "Body Fat Percentage", Unit = "%" };

                dbContext.Users.Add(user);
                dbContext.MeasurementTypes.Add(measurementType);
                await dbContext.SaveChangesAsync();

                testUserId = user.Id;
                testTypeId = measurementType.Id;
            }

            // === 1. ARRANGE ===
            _client.DefaultRequestHeaders.Remove("X-Test-Claim-NameIdentifier");
            _client.DefaultRequestHeaders.Add("X-Test-Claim-NameIdentifier", testUserId.ToString());

            var newLog = new MeasurementLog
            {
                UserId = testUserId,
                MeasurementTypeId = testTypeId,
                Value = 15.5f,
                Date = DateTime.UtcNow
            };

            // === 2. ACT (POST) ===
            
            var postResponse = await _client.PostAsJsonAsync("/GymTracker/Measurements", newLog);
            var postErrorText = await postResponse.Content.ReadAsStringAsync();

            // === 3. ASSERT (POST) ===
            Assert.True(postResponse.IsSuccessStatusCode, $"Сервер вернул ошибку при POST: {postErrorText}");

            // === 4. ACT (GET) ===
            var getResponse = await _client.GetAsync($"GymTracker/Measurements/UsersMeasurements");
            var getErrorText = await getResponse.Content.ReadAsStringAsync();

            // === 5. ASSERT (GET) ===
            Assert.True(getResponse.IsSuccessStatusCode, $"Сервер вернул ошибку при GET: {getErrorText}");

            var retrievedLogs = await getResponse.Content.ReadFromJsonAsync<List<MeasurementLog>>();

            Assert.NotNull(retrievedLogs);
            Assert.Single(retrievedLogs); 
            Assert.Equal(15.5f, retrievedLogs[0].Value);
            Assert.Equal(testTypeId, retrievedLogs[0].MeasurementTypeId);
        }
    }
}