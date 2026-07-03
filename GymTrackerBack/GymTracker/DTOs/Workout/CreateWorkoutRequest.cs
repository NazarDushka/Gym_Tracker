namespace GymTracker.DTOs.Workout
{
    public class CreateWorkoutRequest
    {
        public string? Notes { get; set; }
        public DateTime Date { get; set; }
        public List<CreateWorkoutSetRequest> WorkoutSets { get; set; } = new List<CreateWorkoutSetRequest>();
    }
}
