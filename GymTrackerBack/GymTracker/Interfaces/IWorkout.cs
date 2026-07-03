using GymTracker.Models;

namespace GymTracker.Interfaces
{
    public interface IWorkout 
    {
        Task<IEnumerable<Workout>> GetAll();
        Task<Workout> Get(int id);
        Task<Workout> GetUsersWorkoutById(int userId,int id);
        Task<IEnumerable<Workout>> GetUsersWorkouts(int userId);
        Task Add(Workout workout);
        Task<Workout> GetUsersLastWorkout(int userId);
        void Update(Workout workout);
        void Delete(Workout workout);
    }
}
