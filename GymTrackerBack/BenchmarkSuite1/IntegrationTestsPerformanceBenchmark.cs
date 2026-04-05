using BenchmarkDotNet.Attributes;
using GymTracker.IntegrationTests;
using GymTracker.Models;
using GymTracker.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BenchmarkSuite1
{
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 2, iterationCount: 5)]
    public class IntegrationTestsPerformanceBenchmark
    {
        private GymTrackerWebApplicationFactory? _factory;
        private HttpClient? _client;

        [GlobalSetup]
        public void Setup()
        {
            _factory = new GymTrackerWebApplicationFactory();
            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [Benchmark(Description = "Database Reset with DatabaseResetHelper")]
        public async Task DatabaseResetBenchmark()
        {
            // Инициализация тестовых данных
            await DatabaseResetHelper.ResetDatabaseAsync(_factory!.Services);

            using (var scope = _factory!.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                var user = new User 
                { 
                    FullName = "BenchmarkUser", 
                    PasswordHash = "123", 
                    Email = $"bench_{Guid.NewGuid()}@test.com" 
                };
                var exercise = new Exercise { Name = "Bench Press" };

                dbContext.Users.Add(user);
                dbContext.Exercises.Add(exercise);
                await dbContext.SaveChangesAsync();
            }
        }

        [Benchmark(Description = "Full HTTP Request-Response Cycle")]
        public async Task FullIntegrationBenchmark()
        {
            await DatabaseResetHelper.ResetDatabaseAsync(_factory!.Services);

            int testUserId;
            int testTypeId;

            // Подготовка данных
            using (var scope = _factory!.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                var user = new User 
                { 
                    FullName = "BenchmarkTester", 
                    PasswordHash = "123", 
                    Email = $"bench_{Guid.NewGuid()}@test.com" 
                };
                var measurementType = new MeasurementType 
                { 
                    Name = "Body Fat Percentage", 
                    Unit = "%" 
                };

                dbContext.Users.Add(user);
                dbContext.MeasurementTypes.Add(measurementType);
                await dbContext.SaveChangesAsync();

                testUserId = user.Id;
                testTypeId = measurementType.Id;
            }

            // POST запрос
            var newLog = new MeasurementLog
            {
                UserId = testUserId,
                MeasurementTypeId = testTypeId,
                Value = 15.5f,
                Date = DateTime.UtcNow
            };

            var postResponse = await _client!.PostAsJsonAsync("/api/Measurements", newLog);
            await postResponse.Content.ReadAsStringAsync();

            // GET запрос
            var getResponse = await _client.GetAsync($"/api/users/{testUserId}/measurements");
            await getResponse.Content.ReadFromJsonAsync<List<MeasurementLog>>();
        }
    }
}
