using GymTracker.Models;

namespace GymTracker.Interfaces
{
    public interface IExercise
    {
        Task<IEnumerable<Exercise>> GetAllExercises();
        Task<Exercise> GetExerciseById(int id);
        Task AddExercise(Exercise exercise);
        Task UpdateExercise(Exercise exercise);
        Task DeleteExercise(int id);
    }
}
