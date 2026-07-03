namespace GymTracker.DTOs.Workout
{
    public class WorkoutSetDto
    {
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; }
        public int Reps { get; set; }
        public double Weight { get; set; }
    }
}
