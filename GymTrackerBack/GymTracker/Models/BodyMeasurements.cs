namespace GymTracker.Models
{
    public class BodyMeasurements
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public float? BodyFatPercentage { get; set; }
        public float? Height { get; set; }
        public float? Weight { get; set; }     
        public float? Chest { get; set; }     
        public float? Waist { get; set; }
        public float? Biceps { get; set; }
        public float? Forearms { get; set; }
        public float? Hips { get; set; }
        public float? Legs { get; set; }
        public float? Calves { get; set; }

        public int UserId { get; set; }

    }
}
