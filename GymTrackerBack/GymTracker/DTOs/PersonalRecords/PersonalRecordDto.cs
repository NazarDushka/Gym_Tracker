namespace GymTracker.DTOs.PersonalRecords
{
    public class PersonalRecordDto
    {
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;

        public float CalculatedMaxLift { get; set; }
        public float Weight { get; set; }
        public int Reps { get; set; }

        public DateTime AchievedDate { get; set; }
    }
}