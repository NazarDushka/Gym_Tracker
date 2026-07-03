namespace GymTracker.DTOs.Workout
{
    public class UpdateWorkoutRequest
    {
        public string? Notes { get; set; }
        public DateTime Date { get; set; }
        public List<UpdateWorkoutSetRequest> WorkoutSets { get; set; } = new List<UpdateWorkoutSetRequest>();
    }

    public class UpdateWorkoutSetRequest
    {
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        public int Reps { get; set; }
        public float Weight { get; set; }
    }
}
