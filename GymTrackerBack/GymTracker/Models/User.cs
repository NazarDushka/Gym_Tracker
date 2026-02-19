namespace GymTracker.Models
{
    public class User
    {
        public int? Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Workout>? Workouts { get; set; }
        public ICollection<BodyMeasurement>? BodyMeasurements { get; set; }
    }
}
