using GymTracker.Interfaces;
using GymTracker.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Repository
{
    public class WorkoutRepository : IWorkout
    {
        readonly WorkoutDbContext _db;
        public WorkoutRepository(WorkoutDbContext db)
        {
            _db = db;
        }
        public async Task Add(Workout workout)
        {
            await _db.Workouts.AddAsync(workout);
        }

        public void Delete(Workout workout)
        {
            _db.Workouts.Remove(workout);
        }

        public async Task<IEnumerable<Workout>> GetAll()
        {
            return await _db.Workouts.Include(w => w.Sets).ThenInclude(e => e.Exercise).ToListAsync();
        }

        public async Task<Workout> Get(int id)
        {
            return await _db.Workouts.Include(w => w.Sets).ThenInclude(e => e.Exercise).FirstOrDefaultAsync(x => x.Id == id);
        }

        public void Update(Workout workout)
        {
            _db.Workouts.Update(workout);
        }

        public async Task<Workout> GetMyLastWorkout(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid user ID.");
            }
            else if (userId == null)
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }
            var lastWorkout = await _db.Workouts
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.Date)
                .Include(w => w.Sets)
                .ThenInclude(e => e.Exercise)
                .FirstOrDefaultAsync();
            return lastWorkout ?? throw new KeyNotFoundException("No workouts found for this user.");
        }
        public async Task<Workout> GetMyWorkoutById(int userId, int id)
        {
            return await _db.Workouts
                .Include(w => w.Sets)
                .ThenInclude(e => e.Exercise)
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        }
        public async Task<IEnumerable<Workout>> GetMyWorkouts(int userId)
        {
            return await _db.Workouts
                .Where(w => w.UserId == userId)
                .Include(w => w.Sets)
                .ThenInclude(e => e.Exercise)
                .ToListAsync();
        }
    }
}
