using GymTracker.Interfaces;
using GymTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Repository
{
    public class ExerciseRepository : IExercise
    {
        private readonly WorkoutDbContext _workoutDbContext;

        public ExerciseRepository(WorkoutDbContext workoutDbContext) {
            _workoutDbContext = workoutDbContext;
        }

        public async Task<IEnumerable<Exercise>> GetAllExercises() 
        {
            return await _workoutDbContext.Exercises.ToListAsync();
        }

        public async Task<Exercise> GetExerciseById(int id) 
        {
            return await _workoutDbContext.Exercises.FindAsync(id);
        }

        public async Task AddExercise(Exercise exercise) 
        {
            await _workoutDbContext.Exercises.AddAsync(exercise);
        }

        public async Task UpdateExercise(Exercise exercise) 
        {
            _workoutDbContext.Exercises.Update(exercise);
            await _workoutDbContext.SaveChangesAsync();
        }

        public async Task DeleteExercise(int id) 
        {
            var exercise = await _workoutDbContext.Exercises.FindAsync(id);
            if (exercise != null) 
            {
                _workoutDbContext.Exercises.Remove(exercise);
                await _workoutDbContext.SaveChangesAsync();
            }
        }
    }
}
