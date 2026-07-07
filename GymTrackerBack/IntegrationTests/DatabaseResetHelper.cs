using GymTracker.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace GymTracker.IntegrationTests
{
    /// <summary>
    /// Helper for quick cleanup of the test DB between tests.
    /// Uses efficient cleanup instead of schema recreation.
    /// </summary>
    public class DatabaseResetHelper
    {
        public static async Task ResetDatabaseAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();

                await dbContext.Database.EnsureDeletedAsync();

                await dbContext.Database.EnsureCreatedAsync();
            }
        }

    }
}
