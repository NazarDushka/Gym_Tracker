using GymTracker.DTOs.Workout;

namespace GymTracker.Services.Workouts
{
    public interface IWorkoutService
    {
        Task<IEnumerable<WorkoutDto>> GetUsersWorkoutsAsync(int userId);
        Task<WorkoutDto?> GetWorkoutByIdAsync(int userId, int workoutId);
        Task<WorkoutDto> CreateWorkoutAsync(int userId, CreateWorkoutRequest request);
        Task UpdateWorkoutAsync(int userId, int workoutId, UpdateWorkoutRequest request);
        Task DeleteWorkoutAsync(int userId, int workoutId);
        Task<WorkoutDto?> GetLastUsersWorkoutAsync(int userId);
    }
}