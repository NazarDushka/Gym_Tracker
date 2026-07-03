namespace GymTracker.DTOs.Workout
{
    public class CreateWorkoutSetRequest
    {
        public int ExerciseId { get; set; }
        public int Reps { get; set; }
        public float Weight { get; set; }
    }
}