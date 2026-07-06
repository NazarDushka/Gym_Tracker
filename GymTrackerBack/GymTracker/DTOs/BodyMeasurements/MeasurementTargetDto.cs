namespace GymTracker.DTOs.BodyMeasurements
{
    public class MeasurementTargetDto
    {
        public Guid MeasurementTypeId { get; set; }
        public float TargetValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public bool IsActive { get; set; }
    }
}
