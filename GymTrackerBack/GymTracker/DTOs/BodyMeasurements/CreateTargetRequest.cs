namespace GymTracker.DTOs.BodyMeasurements
{
    public class CreateTargetRequest
    {
        public Guid MeasurementTypeId { get; set; }
        public float TargetValue { get; set; }
        public DateTime? Deadline { get; set; }
    }
}
