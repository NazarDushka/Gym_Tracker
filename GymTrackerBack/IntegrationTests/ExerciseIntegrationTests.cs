using GymTracker.Models;
using GymTracker.Models;
using GymTracker.Repository;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace GymTracker.IntegrationTests
{
    public class ExerciseIntegrationTests : IClassFixture<GymTrackerWebApplicationFactory>
    {
        private readonly GymTrackerWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public ExerciseIntegrationTests(GymTrackerWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
        }

        [Fact]
        public async Task GetExercises_ShouldReturnAllExercises()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                var exercise1 = new Exercise { Name = "Bench Press", Description = "Chest exercise", MuscleGroup = "Chest" };
                var exercise2 = new Exercise { Name = "Squat", Description = "Leg exercise", MuscleGroup = "Legs" };
                var exercise3 = new Exercise { Name = "Deadlift", Description = "Full body exercise", MuscleGroup = "Back" };

                dbContext.Exercises.AddRange(exercise1, exercise2, exercise3);
                await dbContext.SaveChangesAsync();
            }

            // === 1. ACT ===
            var response = await _client.GetAsync("/GetExercises");
            var errorText = await response.Content.ReadAsStringAsync();

            // === 2. ASSERT ===
            Assert.True(response.IsSuccessStatusCode, $"Сервер упал с ошибкой: {errorText}");

            var exercises = await response.Content.ReadFromJsonAsync<List<Exercise>>();
            Assert.NotNull(exercises);
            Assert.Equal(3, exercises.Count);
            Assert.Contains(exercises, e => e.Name == "Bench Press");
            Assert.Contains(exercises, e => e.Name == "Squat");
            Assert.Contains(exercises, e => e.Name == "Deadlift");
        }

        [Fact]
        public async Task GetExerciseById_ShouldReturnExerciseWithCorrectId()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            int testExerciseId;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                var exercise = new Exercise 
                { 
                    Name = "Bench Press", 
                    Description = "Classic chest exercise", 
                    MuscleGroup = "Chest" 
                };

                dbContext.Exercises.Add(exercise);
                await dbContext.SaveChangesAsync();

                testExerciseId = exercise.Id;
            }

            // === 1. ACT ===
            var response = await _client.GetAsync($"/GetExercise/{testExerciseId}");
            var errorText = await response.Content.ReadAsStringAsync();

            // === 2. ASSERT ===
            Assert.True(response.IsSuccessStatusCode, $"Сервер упал с ошибкой: {errorText}");

            var retrievedExercise = await response.Content.ReadFromJsonAsync<Exercise>();
            Assert.NotNull(retrievedExercise);
            Assert.Equal(testExerciseId, retrievedExercise.Id);
            Assert.Equal("Bench Press", retrievedExercise.Name);
            Assert.Equal("Chest", retrievedExercise.MuscleGroup);
        }

        [Fact]
        public async Task GetExerciseById_WithInvalidId_ShouldReturnNotFound()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            // === 1. ACT ===
            var response = await _client.GetAsync($"/GetExercise/999");

            // === 2. ASSERT ===
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddExercise_ShouldCreateNewExerciseInDatabase()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            // === 1. ARRANGE ===
            var newExercise = new Exercise
            {
                Name = "Pull-ups",
                Description = "Upper body exercise",
                MuscleGroup = "Back"
            };

            // === 2. ACT ===
            var response = await _client.PostAsJsonAsync("/AddExercise", newExercise);
            var errorText = await response.Content.ReadAsStringAsync();

            // === 3. ASSERT ===
            Assert.True(response.IsSuccessStatusCode, $"Сервер упал с ошибкой: {errorText}");

            // Проверяем, что упражнение было сохранено в БД
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                var savedExercise = dbContext.Exercises.FirstOrDefault(e => e.Name == "Pull-ups");
                
                Assert.NotNull(savedExercise);
                Assert.Equal("Pull-ups", savedExercise.Name);
                Assert.Equal("Upper body exercise", savedExercise.Description);
                Assert.Equal("Back", savedExercise.MuscleGroup);
            }
        }

        [Fact]
        public async Task UpdateExercise_ShouldModifyExistingExercise()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            int testExerciseId;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                var exercise = new Exercise 
                { 
                    Name = "Bench Press", 
                    Description = "Classic chest exercise", 
                    MuscleGroup = "Chest" 
                };

                dbContext.Exercises.Add(exercise);
                await dbContext.SaveChangesAsync();

                testExerciseId = exercise.Id;
            }

            // === 1. ARRANGE ===
            var updatedExercise = new Exercise
            {
                Name = "Incline Bench Press",
                Description = "Upper chest exercise",
                MuscleGroup = "Upper Chest"
            };

            // === 2. ACT ===
            var response = await _client.PutAsJsonAsync($"/UpdateExercise/{testExerciseId}", updatedExercise);
            var errorText = await response.Content.ReadAsStringAsync();

            // === 3. ASSERT ===
            Assert.True(response.IsSuccessStatusCode, $"Сервер упал с ошибкой: {errorText}");

            // Проверяем, что упражнение было обновлено в БД
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                var savedExercise = dbContext.Exercises.FirstOrDefault(e => e.Id == testExerciseId);
                
                Assert.NotNull(savedExercise);
                Assert.Equal("Incline Bench Press", savedExercise.Name);
                Assert.Equal("Upper chest exercise", savedExercise.Description);
                Assert.Equal("Upper Chest", savedExercise.MuscleGroup);
            }
        }

        [Fact]
        public async Task UpdateExercise_WithInvalidId_ShouldReturnNotFound()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            // === 1. ARRANGE ===
            var updatedExercise = new Exercise
            {
                Name = "Updated Exercise",
                Description = "Some description",
                MuscleGroup = "Some Muscle"
            };

            // === 2. ACT ===
            var response = await _client.PutAsJsonAsync($"/UpdateExercise/999", updatedExercise);

            // === 3. ASSERT ===
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteExercise_ShouldRemoveExerciseFromDatabase()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            int testExerciseId;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                var exercise = new Exercise 
                { 
                    Name = "Bench Press", 
                    Description = "Chest exercise", 
                    MuscleGroup = "Chest" 
                };

                dbContext.Exercises.Add(exercise);
                await dbContext.SaveChangesAsync();

                testExerciseId = exercise.Id;
            }

            // === 1. ACT ===
            var response = await _client.DeleteAsync($"/DeleteExercise/{testExerciseId}");
            var errorText = await response.Content.ReadAsStringAsync();

            // === 2. ASSERT ===
            Assert.True(response.IsSuccessStatusCode, $"Сервер упал с ошибкой: {errorText}");

            // Проверяем, что упражнение было удалено из БД
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                var deletedExercise = dbContext.Exercises.FirstOrDefault(e => e.Id == testExerciseId);
                
                Assert.Null(deletedExercise);
            }
        }

        [Fact]
        public async Task DeleteExercise_WithInvalidId_ShouldReturnOk()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            // === 1. ACT ===
            var response = await _client.DeleteAsync($"/DeleteExercise/999");

            // === 2. ASSERT ===
            // Контроллер всегда возвращает Ok, даже если упражнение не существует
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetExercises_WhenDatabaseIsEmpty_ShouldReturnEmptyList()
        {
            // === 0. SEEDING ===
            await DatabaseResetHelper.ResetDatabaseAsync(_factory.Services);

            // === 1. ACT ===
            var response = await _client.GetAsync("/GetExercises");
            var errorText = await response.Content.ReadAsStringAsync();

            // === 2. ASSERT ===
            Assert.True(response.IsSuccessStatusCode, $"Сервер упал с ошибкой: {errorText}");

            var exercises = await response.Content.ReadFromJsonAsync<List<Exercise>>();
            Assert.NotNull(exercises);
            Assert.Empty(exercises);
        }
    }
}
