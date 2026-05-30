namespace GymTracker.Models
{
    public class MeasurementType
    {
        public Guid Id { get; set; }
        public string Name { get; set; } 
        public string Unit { get; set; } 

        public ICollection<MeasurementTarget> Targets { get; set; }
        public ICollection<MeasurementLog> Logs { get; set; }
    }

    public class MeasurementTarget
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public Guid MeasurementTypeId { get; set; }
        public MeasurementType? MeasurementType { get; set; }

        public float TargetValue { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public bool IsActive { get; set; }
    }

    public class MeasurementLog
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }

        public Guid MeasurementTypeId { get; set; }
        public MeasurementType? MeasurementType { get; set; } 

        public float Value { get; set; } 
        public DateTime Date { get; set; }
    }
}