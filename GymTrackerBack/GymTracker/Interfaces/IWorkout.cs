using GymTracker.Models;

namespace GymTracker.Interfaces
{
    public interface IWorkout 
    {
        Task<IEnumerable<Workout>> GetAll();
        Task<Workout> Get(int id);
        Task<Workout> GetMyWorkoutById(int userId,int id);
        Task<IEnumerable<Workout>> GetMyWorkouts(int userId);
        Task Add(Workout workout);
        Task<Workout> GetMyLastWorkout(int userId);
        void Update(Workout workout);
        void Delete(Workout workout);
    }
}
