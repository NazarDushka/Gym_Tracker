using GymTracker.DTOs.Workout;
using GymTracker.Interfaces;
using GymTracker.Models;

namespace GymTracker.Services
{
    public class WorkoutService:IWorkoutService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WorkoutService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<WorkoutDto>> GetUsersWorkoutsAsync(int userId)
        {
            var workouts = await _unitOfWork.Workout.GetUsersWorkouts(userId);
            return workouts
                .OrderByDescending(w => w.Date)
                .Select(MapToDto);
        }

        public async Task<WorkoutDto?> GetWorkoutByIdAsync(int userId, int workoutId)
        {
            var workout = await _unitOfWork.Workout.GetUsersWorkoutById(userId, workoutId);
            if (workout == null || workout.UserId != userId)
                return null;
            return MapToDto(workout);
        }

        public async Task<WorkoutDto> CreateWorkoutAsync(int userId, CreateWorkoutRequest request)
        {
            var workout = new Workout
            {
                UserId = userId,
                Date = request.Date,
                Notes = request.Notes,
                Sets = request.WorkoutSets.Select(s => new WorkoutSet
                {
                    ExerciseId = s.ExerciseId,
                    Reps = s.Reps,
                    Weight = s.Weight
                }).ToList()
            };
            await _unitOfWork.Workout.Add(workout);
            foreach (var set in workout.Sets)
            {
                await _unitOfWork.PersonalRecords.AddOrUpdatePersonalRecord(
                    userId, set.ExerciseId, set);
            }
            await _unitOfWork.CompleteAsync();

            var createdWorkout = await _unitOfWork.Workout.GetUsersWorkoutById(userId, workout.Id);
            return MapToDto(createdWorkout);
        }

        public async Task UpdateWorkoutAsync(int userId, int workoutId, UpdateWorkoutRequest request)
        {
            var existingWorkout = await _unitOfWork.Workout.GetUsersWorkoutById(userId, workoutId);
            if (existingWorkout == null || existingWorkout.UserId != userId)
                throw new KeyNotFoundException("Workout not found.");
            existingWorkout.Date = request.Date;
            existingWorkout.Notes = request.Notes;
            var incomingSetIds = request.WorkoutSets.Select(s => s.Id).ToList();
            var setsToRemove = existingWorkout.Sets
                .Where(existingSet => !incomingSetIds.Contains(existingSet.Id))
                .ToList();
            foreach (var setToRemove in setsToRemove)
            {
                existingWorkout.Sets.Remove(setToRemove);
            }
            foreach (var incomingSet in request.WorkoutSets)
            {
                var existingSet = existingWorkout.Sets.FirstOrDefault(s => s.Id == incomingSet.Id);
                if (existingSet != null && existingSet.Id != 0)
                {
                    existingSet.Reps = incomingSet.Reps;
                    existingSet.Weight = incomingSet.Weight;
                    existingSet.ExerciseId = incomingSet.ExerciseId;
                }
                else
                {
                    existingWorkout.Sets.Add(new WorkoutSet
                    {
                        Reps = incomingSet.Reps,
                        Weight = incomingSet.Weight,
                        ExerciseId = incomingSet.ExerciseId
                    });
                }
            }
            await _unitOfWork.CompleteAsync();
            foreach (var set in existingWorkout.Sets)
            {
                await _unitOfWork.PersonalRecords.AddOrUpdatePersonalRecord(
                    userId, set.ExerciseId, set);
            }
            await _unitOfWork.CompleteAsync();

            foreach (var set in existingWorkout.Sets)
            {
                await _unitOfWork.PersonalRecords.AddOrUpdatePersonalRecord(
                    userId, set.ExerciseId, set);
            }
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteWorkoutAsync(int userId, int workoutId)
        {
            var workoutToDelete = await _unitOfWork.Workout.GetUsersWorkoutById(userId, workoutId);
            if (workoutToDelete == null || workoutToDelete.UserId != userId)
                throw new KeyNotFoundException("Workout not found.");
            _unitOfWork.Workout.Delete(workoutToDelete);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<WorkoutDto?> GetLastUsersWorkoutAsync(int userId)
        {
            try
            {
                var lastWorkout = await _unitOfWork.Workout.GetUsersLastWorkout(userId);
                return lastWorkout != null ? MapToDto(lastWorkout) : null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // ===== Private mapping methods =====
        private static WorkoutDto MapToDto(Workout workout)
        {
            return new WorkoutDto
            {
                Id = workout.Id,
                Date = workout.Date,
                Notes = workout.Notes,
                WorkoutSets = workout.Sets.Select(s => new WorkoutSetDto
                {
                    Id = s.Id,
                    Reps = s.Reps,
                    Weight = s.Weight,
                    ExerciseId = s.ExerciseId,
                    ExerciseName = s.Exercise?.Name ?? string.Empty
                }).ToList()
            };
        }
    }
}
