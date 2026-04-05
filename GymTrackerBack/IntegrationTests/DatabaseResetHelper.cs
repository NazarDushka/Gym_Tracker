using GymTracker.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace GymTracker.IntegrationTests
{
    /// <summary>
    /// Помощник для быстрой очистки тестовой БД между тестами.
    /// Использует эффективную очистку вместо пересоздания схемы.
    /// </summary>
    public class DatabaseResetHelper
    {
        public static async Task ResetDatabaseAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
                
                // Для In-Memory БД просто удаляем все записи эффективно
                await ClearAllTablesAsync(dbContext);
            }
        }

        private static async Task ClearAllTablesAsync(WorkoutDbContext dbContext)
        {
            // Порядок удаления важен из-за внешних ключей!
            // Удаляем в порядке, обратном созданию (от зависимых к независимым)
            
            dbContext.MeasurementLogs.RemoveRange(dbContext.MeasurementLogs);
            dbContext.MeasurementTargets.RemoveRange(dbContext.MeasurementTargets);
            dbContext.PersonalRecords.RemoveRange(dbContext.PersonalRecords);
            dbContext.WorkoutSets.RemoveRange(dbContext.WorkoutSets);
            dbContext.Workouts.RemoveRange(dbContext.Workouts);
            dbContext.MeasurementTypes.RemoveRange(dbContext.MeasurementTypes);
            dbContext.Exercises.RemoveRange(dbContext.Exercises);
            dbContext.Users.RemoveRange(dbContext.Users);

            await dbContext.SaveChangesAsync();
        }
    }
}
