namespace GymTracker.DTOs.BodyMeasurements
{
    public class CreateMeasurementLogRequest
    {
        public Guid MeasurementTypeId { get; set; }
        public float Value { get; set; }
        public DateTime Date { get; set; }
    }
}
