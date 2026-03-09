using GymTracker.Interfaces;
using GymTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Repository
{
    public class BodyMeasurementsRepository: IBodyMeasurementRep
    {
        private readonly WorkoutDbContext _workoutDbContext;

        public BodyMeasurementsRepository(WorkoutDbContext workoutDbContext)
        {
            _workoutDbContext = workoutDbContext;
        }

        public async Task<IEnumerable<BodyMeasurements>> GetBodyMeasurementsByUserId(int userId)
            {
                return await _workoutDbContext.BodyMeasurements.Where(bm => bm.UserId == userId).ToListAsync();
        }

        public async Task AddBodyMeasurement(BodyMeasurements bodyMeasurement)
        {
            await _workoutDbContext.BodyMeasurements.AddAsync(bodyMeasurement);
        }

        public async Task UpdateBodyMeasurement(BodyMeasurements bodyMeasurement)
        {
            _workoutDbContext.BodyMeasurements.Update(bodyMeasurement);
            await _workoutDbContext.SaveChangesAsync();
        }

        public async Task DeleteBodyMeasurement(int id)
        {
            var bodyMeasurement = await _workoutDbContext.BodyMeasurements.FindAsync(id);
            if (bodyMeasurement != null)
            {
                _workoutDbContext.BodyMeasurements.Remove(bodyMeasurement);
                await _workoutDbContext.SaveChangesAsync();
            }
        }

        public async Task<BodyMeasurements> GetBodyMeasurementById(int id)
        {
            return await _workoutDbContext.BodyMeasurements.FindAsync(id);
        }
    

    }
}
