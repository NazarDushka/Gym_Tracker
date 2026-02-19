using GymTracker.Models;

namespace GymTracker.Interfaces
{
    public interface IExercise
    {
        Task<IEnumerable<Exercise>> GetExercises();
    }
}
