using GymTracker.Interfaces;
using GymTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Repository
{
    public class MeasurementsRepository : IMeasurementRepository
    {
        private readonly WorkoutDbContext _workoutDbContext;

        public MeasurementsRepository(WorkoutDbContext workoutDbContext)
        {
            _workoutDbContext = workoutDbContext;
        }

        public async Task<IEnumerable<MeasurementType>> GetAllTypesAsync()
        {
            return await _workoutDbContext.MeasurementTypes
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<MeasurementLog>> GetLastLogsForUserAsync(int userId)
        { 
        return await _workoutDbContext.MeasurementLogs
            .AsNoTracking()
            .Include(l => l.MeasurementType)
            .Where(l => l.UserId == userId)
            .GroupBy(l => l.MeasurementTypeId)
            .Select(g => g.OrderByDescending(l => l.Date).FirstOrDefault())
            .ToListAsync();
        }

        public async Task<IEnumerable<MeasurementLog>> GetLogsByUserIdAsync(int userId)
        {
            return await _workoutDbContext.MeasurementLogs
                .Include(l => l.MeasurementType)
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Date)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<MeasurementLog>> GetLogsByTypeAsync(int userId, Guid typeId)
        {
            return await _workoutDbContext.MeasurementLogs
                .Where(l => l.UserId == userId && l.MeasurementTypeId == typeId)
                .OrderBy(l => l.Date)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<MeasurementLog?> GetLogByIdAsync(Guid logId)
        {
            return await _workoutDbContext.MeasurementLogs
                .Include(l => l.MeasurementType)
                .FirstOrDefaultAsync(l => l.Id == logId);
        }

        public async Task AddLogAsync(MeasurementLog log)
        {
            await _workoutDbContext.MeasurementLogs.AddAsync(log);
        }

        public async Task DeleteLogAsync(Guid logId)
        {
            var log = await _workoutDbContext.MeasurementLogs.FindAsync(logId);
            if (log != null)
            {
                _workoutDbContext.MeasurementLogs.Remove(log);
            }
        }

        public async Task<IEnumerable<MeasurementTarget>> GetActiveTargetsByUserIdAsync(int userId)
        {
            return await _workoutDbContext.MeasurementTargets
                .Include(t => t.MeasurementType)
                .Where(t => t.UserId == userId && t.IsActive)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<MeasurementTarget> GetActiveTargetByMeasurementTypeAsync(int userId, Guid measurementTypeId)
        {
            return await _workoutDbContext.MeasurementTargets
                .Include(t => t.MeasurementType)
                .Where(t => t.UserId == userId && t.MeasurementTypeId == measurementTypeId && t.IsActive)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<MeasurementTarget?> GetTargetByIdAsync(int targetId)
        {
            return await _workoutDbContext.MeasurementTargets
                .Include(t => t.MeasurementType)
                .FirstOrDefaultAsync(t => t.Id == targetId);
        }
        public async Task AddTargetAsync(MeasurementTarget target)
        {
            await _workoutDbContext.MeasurementTargets.AddAsync(target);
        }

        public async Task DeactivateTargetAsync(int targetId, MeasurementTarget target)
        {
                target.IsActive = false;
                _workoutDbContext.MeasurementTargets.Update(target);
        }
    }
}
