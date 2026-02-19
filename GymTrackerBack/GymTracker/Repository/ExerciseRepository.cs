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

        public async Task<IEnumerable<Exercise>> GetExercises() 
        {
            return await _workoutDbContext.Exercises.ToListAsync();
        }
    }
}
