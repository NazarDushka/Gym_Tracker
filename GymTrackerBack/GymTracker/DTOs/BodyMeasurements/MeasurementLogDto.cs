namespace GymTracker.DTOs.BodyMeasurements
{
    public class MeasurementLogDto
    {
        public Guid MeasurementTypeId { get; set; }
        public float Value { get; set; }
        public DateTime Date { get; set; }
    }
}
