namespace GymTracker.Models
{
    public class Workout
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string? Notes { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public ICollection<WorkoutSet> Sets { get; set; }
    }
}
