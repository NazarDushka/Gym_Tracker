namespace GymTracker.Models
{
    public class BodyMeasurement
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public float Weight { get; set; }     
        public float? Chest { get; set; }     
        public float? Waist { get; set; }
        public float? Biceps { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
