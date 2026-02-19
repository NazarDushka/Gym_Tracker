namespace GymTracker.Models
{
    public class WorkoutSet
    {
        public int Id { get; set; }
        public int Reps { get; set; }
        public float Weight { get; set; }

        public int ExerciseId { get; set; }
        public Exercise Exercise { get; set; }

        public int WorkoutId { get; set; }
        public Workout Workout { get; set; }
    }
}
