namespace GymTracker.DTOs.Workout
{
    public class WorkoutDto
    {
        public int Id { get; set; }
        public string? Notes { get; set; }
        public DateTime Date { get; set; }
        public List<WorkoutSetDto> WorkoutSets { get; set; } = new List<WorkoutSetDto>();
    }
}
    